/*
 * Order application service. It validates customer/product references and
 * snapshots the current product price into each order item.
 * Checkout/payment workflows are handled in CheckoutService; this service keeps
 * direct order administration focused and testable.
 */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponse>> GetAllAsync(int? customerId, CancellationToken cancellationToken);
    Task<OrderResponse?> GetByIdAsync(int id, int? customerId, CancellationToken cancellationToken);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> UpdateAsync(int id, UpdateOrderRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class OrderService(OrderFlowDbContext db) : IOrderService
{
    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(int? customerId, CancellationToken ct) =>
        await Query(customerId).OrderByDescending(x => x.Id).Select(x => ToResponse(x)).ToListAsync(ct);
    public Task<OrderResponse?> GetByIdAsync(int id, int? customerId, CancellationToken ct) => Query(customerId).Where(x => x.Id == id).Select(x => ToResponse(x)).SingleOrDefaultAsync(ct);
    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        await ValidateReferencesAsync(request.CustomerId, request.Items, ct);
        var products = await ProductsByIdAsync(request.Items, ct);
        var order = new Order { CustomerId = request.CustomerId, OrderDate = DateTime.UtcNow, Status = OrderStatus.Pending };
        foreach (var item in request.Items) order.Items.Add(new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = products[item.ProductId].Price });
        db.Orders.Add(order); await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(order.Id, null, ct))!;
    }
    public async Task<OrderResponse?> UpdateAsync(int id, UpdateOrderRequest request, CancellationToken ct)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, ct); if (order is null) return null;
        await ValidateReferencesAsync(request.CustomerId, request.Items, ct);
        var products = await ProductsByIdAsync(request.Items, ct);
        order.CustomerId = request.CustomerId;
        order.Items.Clear();
        foreach (var item in request.Items) order.Items.Add(new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = products[item.ProductId].Price });
        await db.SaveChangesAsync(ct); return (await GetByIdAsync(id, null, ct))!;
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([id], ct); if (order is null) return false;
        db.Orders.Remove(order); await db.SaveChangesAsync(ct); return true;
    }
    private IQueryable<Order> Query(int? customerId = null)
    {
        var query = db.Orders.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Product);
        return customerId is int id ? query.Where(x => x.CustomerId == id) : query;
    }
    private async Task ValidateReferencesAsync(int customerId, IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct)
    {
        if (!await db.Customers.AnyAsync(x => x.Id == customerId, ct)) throw new BusinessRuleException("Customer was not found.");
        if (items.Select(x => x.ProductId).Distinct().Count() != items.Count) throw new BusinessRuleException("Each product may appear only once in an order.");
    }
    private async Task<Dictionary<int, Product>> ProductsByIdAsync(IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct)
    {
        var ids = items.Select(x => x.ProductId).ToArray();
        var products = await db.Products.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        if (products.Count != ids.Length) throw new BusinessRuleException("One or more products were not found.");
        return products;
    }
    private static OrderResponse ToResponse(Order order) => new(order.Id, order.CustomerId, order.OrderDate, order.Status, order.Items.Sum(x => x.UnitPrice * x.Quantity),
        order.Items.Select(x => new OrderItemResponse(x.Id, x.ProductId, x.Product.Name, x.Quantity, x.UnitPrice)).ToArray());
}
