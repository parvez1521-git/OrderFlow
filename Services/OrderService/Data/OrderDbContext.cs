/* EF Core context for OrderService's private database. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrderService.Models;

namespace OrderFlow.OrderService.Data;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CheckoutSagaState> CheckoutSagas => Set<CheckoutSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        modelBuilder.Entity<Order>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CheckoutSagaState>().Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<CheckoutSagaState>().Property(x => x.CurrentStep).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<CheckoutSagaState>().Property(x => x.Status).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<CheckoutSagaState>().HasIndex(x => x.IdempotencyKey).IsUnique();
    }
}
