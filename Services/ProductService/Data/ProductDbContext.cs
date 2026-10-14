/* EF Core context for ProductService's private database. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.ProductService.Models;

namespace OrderFlow.ProductService.Data;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(x => x.Name).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
    }
}
