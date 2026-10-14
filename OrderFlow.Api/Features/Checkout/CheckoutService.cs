/* Checkout service coordinates cart validation, inventory, order creation, payment, and cleanup. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Features.Cart;
using OrderFlow.Api.Features.Inventory;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.Features.Payment;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Checkout;

public interface ICheckoutService
{
    Task<CheckoutResponse> CheckoutAsync(int customerId, CheckoutRequest request, CancellationToken cancellationToken);
}

public sealed class CheckoutService(OrderFlowDbContext db, ICartService carts, IInventoryService inventory, IPaymentGateway payments) : ICheckoutService
{
    public async Task<CheckoutResponse> CheckoutAsync(int customerId, CheckoutRequest request, CancellationToken ct)
    {
        var cart = await carts.GetAsync(customerId, ct);
        if (!cart.Items.Any()) throw new BusinessRuleException("Cart is empty.");

        var stockItems = cart.Items.Select(x => (x.ProductId, x.Quantity)).ToArray();
        await inventory.ReserveStockAsync(stockItems, ct);

        var order = new Order { CustomerId = customerId, OrderDate = DateTime.UtcNow, Status = OrderStatus.Pending };
        foreach (var item in cart.Items)
            order.Items.Add(new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var payment = await payments.ProcessAsync(order.Id, cart.Total, request.PaymentMode, request.IdempotencyKey, ct);
        order.Status = payment.Status == PaymentStatus.Succeeded ? OrderStatus.Confirmed :
            payment.Status == PaymentStatus.Timeout ? OrderStatus.Cancelled : OrderStatus.PaymentFailed;

        if (payment.Status == PaymentStatus.Succeeded) await carts.ClearAsync(customerId, ct);
        else await inventory.ReleaseStockAsync(stockItems, ct);
        await db.SaveChangesAsync(ct);

        var orderResponse = await db.Orders.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Product)
            .Where(x => x.Id == order.Id)
            .Select(x => new OrderResponse(x.Id, x.CustomerId, x.OrderDate, x.Status, x.Items.Sum(i => i.UnitPrice * i.Quantity),
                x.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice)).ToArray()))
            .SingleAsync(ct);
        return new CheckoutResponse(orderResponse, payment, cart);
    }
}
