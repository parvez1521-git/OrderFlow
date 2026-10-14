/* EF Core context for InventoryService's private database. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.InventoryService.Models;

namespace OrderFlow.InventoryService.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<InventoryItem>().HasIndex(x => x.ProductId).IsUnique();
}
