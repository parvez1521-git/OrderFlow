/* Controller for product catalog endpoints. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.ProductService.Data;
using OrderFlow.ProductService.Dtos;
using OrderFlow.ProductService.Models;
using OrderFlow.ProductService.Services;

namespace OrderFlow.ProductService.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductDbContext db, IProductCache cache) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Product>> GetAll()
    {
        if (await cache.GetProductsAsync() is { } cachedProducts) return cachedProducts;

        var products = await db.Products.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        await cache.SetProductsAsync(products);
        return products;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id) => await db.Products.FindAsync(id) is { } product ? Ok(product) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductRequest request)
    {
        var product = new Product { Name = request.Name.Trim(), Price = request.Price };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        await cache.InvalidateProductsAsync();
        return CreatedAtAction(nameof(GetById), new { product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Update(int id, ProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.Name = request.Name.Trim();
        product.Price = request.Price;
        await db.SaveChangesAsync();
        await cache.InvalidateProductsAsync();
        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        await cache.InvalidateProductsAsync();
        return NoContent();
    }
}
