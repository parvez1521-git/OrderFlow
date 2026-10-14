/* Product application service. It owns product CRUD rules and prevents
 * deletion of products already referenced by an order. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;
namespace OrderFlow.Api.Features.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class ProductService(OrderFlowDbContext db) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken ct) => await db.Products.AsNoTracking().OrderBy(x => x.Id).Select(x => ToResponse(x)).ToListAsync(ct);
    public Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct) => db.Products.AsNoTracking().Where(x => x.Id == id).Select(x => ToResponse(x)).SingleOrDefaultAsync(ct);
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product { Name = request.Name.Trim(), Price = request.Price };
        db.Products.Add(product); await db.SaveChangesAsync(ct); return ToResponse(product);
    }
    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct); if (product is null) return null;
        product.Name = request.Name.Trim(); product.Price = request.Price; await db.SaveChangesAsync(ct); return ToResponse(product);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct); if (product is null) return false;
        if (await db.OrderItems.AnyAsync(x => x.ProductId == id, ct)) throw new BusinessRuleException("A product in an order cannot be deleted.");
        db.Products.Remove(product); await db.SaveChangesAsync(ct); return true;
    }
    private static ProductResponse ToResponse(Product product) => new(product.Id, product.Name, product.Price);
}
