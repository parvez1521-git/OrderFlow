/* Cart service owns add/update/remove/clear behavior and server-side prices. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Cart;

public interface ICartService
{
    Task<CartResponse> GetAsync(int customerId, CancellationToken cancellationToken);
    Task<CartResponse> AddAsync(int customerId, CartItemRequest request, CancellationToken cancellationToken);
    Task<CartResponse> UpdateAsync(int customerId, CartItemRequest request, CancellationToken cancellationToken);
    Task<CartResponse> RemoveAsync(int customerId, int productId, CancellationToken cancellationToken);
    Task ClearAsync(int customerId, CancellationToken cancellationToken);
}

public sealed class CartService(OrderFlowDbContext db) : ICartService
{
    public async Task<CartResponse> GetAsync(int customerId, CancellationToken ct) => ToResponse(await GetOrCreateAsync(customerId, ct));

    public async Task<CartResponse> AddAsync(int customerId, CartItemRequest request, CancellationToken ct)
    {
        await EnsureProductAsync(request.ProductId, ct);
        var cart = await GetOrCreateAsync(customerId, ct);
        var item = cart.Items.SingleOrDefault(x => x.ProductId == request.ProductId);
        if (item is null) cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
        else item.Quantity += request.Quantity;
        await db.SaveChangesAsync(ct);
        return ToResponse(await LoadCartAsync(customerId, ct));
    }

    public async Task<CartResponse> UpdateAsync(int customerId, CartItemRequest request, CancellationToken ct)
    {
        await EnsureProductAsync(request.ProductId, ct);
        var cart = await GetOrCreateAsync(customerId, ct);
        var item = cart.Items.SingleOrDefault(x => x.ProductId == request.ProductId);
        if (item is null) cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
        else item.Quantity = request.Quantity;
        await db.SaveChangesAsync(ct);
        return ToResponse(await LoadCartAsync(customerId, ct));
    }

    public async Task<CartResponse> RemoveAsync(int customerId, int productId, CancellationToken ct)
    {
        var cart = await GetOrCreateAsync(customerId, ct);
        var item = cart.Items.SingleOrDefault(x => x.ProductId == productId);
        if (item is not null) cart.Items.Remove(item);
        await db.SaveChangesAsync(ct);
        return ToResponse(await LoadCartAsync(customerId, ct));
    }

    public async Task ClearAsync(int customerId, CancellationToken ct)
    {
        var cart = await GetOrCreateAsync(customerId, ct);
        cart.Items.Clear();
        await db.SaveChangesAsync(ct);
    }

    private async Task<ShoppingCart> GetOrCreateAsync(int customerId, CancellationToken ct)
    {
        if (!await db.Customers.AnyAsync(x => x.Id == customerId, ct)) throw new BusinessRuleException("Customer was not found.");
        var cart = await LoadCartAsync(customerId, ct);
        if (cart is not null) return cart;
        cart = new ShoppingCart { CustomerId = customerId };
        db.ShoppingCarts.Add(cart);
        await db.SaveChangesAsync(ct);
        return await LoadCartAsync(customerId, ct);
    }

    private async Task<ShoppingCart> LoadCartAsync(int customerId, CancellationToken ct) =>
        (await db.ShoppingCarts.Include(x => x.Items).ThenInclude(x => x.Product).SingleOrDefaultAsync(x => x.CustomerId == customerId, ct))!;

    private async Task EnsureProductAsync(int productId, CancellationToken ct)
    {
        if (!await db.Products.AnyAsync(x => x.Id == productId, ct)) throw new BusinessRuleException("Product was not found.");
    }

    private static CartResponse ToResponse(ShoppingCart cart)
    {
        var items = cart.Items.OrderBy(x => x.ProductId).Select(x => new CartItemResponse(x.ProductId, x.Product.Name, x.Product.Price, x.Quantity, x.Product.Price * x.Quantity)).ToArray();
        return new CartResponse(cart.CustomerId, items.Sum(x => x.LineTotal), items);
    }
}
