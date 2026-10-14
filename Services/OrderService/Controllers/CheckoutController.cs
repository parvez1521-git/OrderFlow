/* Phase 10 checkout controller demonstrating synchronous service-to-service HTTP calls. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrderService.Clients;
using OrderFlow.OrderService.Data;
using OrderFlow.OrderService.Dtos;
using OrderFlow.OrderService.Messaging;
using OrderFlow.OrderService.Models;

namespace OrderFlow.OrderService.Controllers;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController(OrderDbContext db, IProductCatalogClient products, IInventoryClient inventory, IPaymentClient payments, INotificationClient notifications, IEventPublisher events) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> Checkout(CheckoutRequest request, CancellationToken ct)
    {
        var existingSaga = await db.CheckoutSagas.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey, ct);
        if (existingSaga?.OrderId is int existingOrderId)
            return Ok(new CheckoutResponse(existingOrderId, existingSaga.Status, "Duplicate", 0, "Duplicate checkout request returned existing saga result."));

        var saga = new CheckoutSagaState { IdempotencyKey = request.IdempotencyKey, CurrentStep = "Started", Status = "Started", StartedAtUtc = DateTime.UtcNow };
        db.CheckoutSagas.Add(saga);
        await db.SaveChangesAsync(ct);

        var reserved = new List<CheckoutItemRequest>();
        try
        {
            var productSnapshots = new List<(ProductSnapshot Product, int Quantity)>();
            foreach (var item in request.Items)
            {
                saga.CurrentStep = "ReadProduct";
                var product = await products.GetProductAsync(item.ProductId, ct);
                productSnapshots.Add((product, item.Quantity));
                saga.CurrentStep = "ReserveInventory";
                await inventory.ReserveAsync(item.ProductId, item.Quantity, ct);
                reserved.Add(item);
                events.Publish("inventory.reserved", new IntegrationEvent(Guid.NewGuid(), "InventoryReserved", 0, DateTime.UtcNow, new { item.ProductId, item.Quantity }));
            }

            saga.CurrentStep = "CreateOrder";
            var order = new Order { CustomerId = request.CustomerId, OrderDateUtc = DateTime.UtcNow, Status = OrderStatus.Pending };
            foreach (var line in productSnapshots)
                order.Items.Add(new OrderItem { ProductId = line.Product.Id, ProductName = line.Product.Name, Quantity = line.Quantity, UnitPrice = line.Product.Price });
            order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);
            db.Orders.Add(order);
            await db.SaveChangesAsync(ct);
            saga.OrderId = order.Id;

            saga.CurrentStep = "ProcessPayment";
            var payment = await payments.ProcessAsync(order.Id, order.Total, request.PaymentMode, request.IdempotencyKey, ct);
            order.Status = payment.Status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase) ? OrderStatus.Confirmed :
                payment.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase) ? OrderStatus.PaymentFailed : OrderStatus.Cancelled;

            if (order.Status == OrderStatus.Confirmed)
            {
                saga.CurrentStep = "ConfirmOrder";
                await notifications.NotifyOrderCreatedAsync(order.Id, request.CustomerEmail, $"Order {order.Id} confirmed for {order.Total:C}.", ct);
                events.Publish("order.confirmed", new IntegrationEvent(Guid.NewGuid(), "OrderConfirmed", order.Id, DateTime.UtcNow, new { request.CustomerEmail, order.Total }));
            }
            else
            {
                saga.CurrentStep = "CompensateReleaseInventory";
                foreach (var item in reserved) await inventory.ReleaseAsync(item.ProductId, item.Quantity, ct);
                events.Publish("payment.failed", new IntegrationEvent(Guid.NewGuid(), "PaymentFailed", order.Id, DateTime.UtcNow, new { request.CustomerEmail, PaymentStatus = payment.Status }));
            }

            saga.Status = order.Status.ToString();
            saga.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Ok(new CheckoutResponse(order.Id, order.Status.ToString(), payment.Status, order.Total, "Checkout completed through synchronous microservice calls."));
        }
        catch (TaskCanceledException)
        {
            foreach (var item in reserved) await inventory.ReleaseAsync(item.ProductId, item.Quantity, CancellationToken.None);
            saga.Status = "Failed";
            saga.FailureReason = "A downstream service timed out.";
            saga.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new ProblemDetails { Title = "A downstream service timed out.", Status = StatusCodes.Status504GatewayTimeout });
        }
        catch (HttpRequestException ex)
        {
            foreach (var item in reserved) await inventory.ReleaseAsync(item.ProductId, item.Quantity, CancellationToken.None);
            saga.Status = "Failed";
            saga.FailureReason = ex.Message;
            saga.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails { Title = "A downstream service is unavailable.", Detail = ex.Message, Status = StatusCodes.Status503ServiceUnavailable });
        }
        catch (DownstreamServiceException ex)
        {
            foreach (var item in reserved) await inventory.ReleaseAsync(item.ProductId, item.Quantity, CancellationToken.None);
            saga.Status = "Failed";
            saga.FailureReason = $"{ex.ServiceName}: {ex.Message}";
            saga.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            return BadRequest(new ProblemDetails { Title = $"{ex.ServiceName} rejected the checkout request.", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    [HttpGet("sagas")]
    public async Task<IReadOnlyList<CheckoutSagaState>> GetSagas(CancellationToken ct) => await db.CheckoutSagas.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync(ct);
}
