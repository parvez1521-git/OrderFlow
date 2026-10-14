/*
 * EF Core database context for the OrderFlow API.
 * It maps C# domain models to SQL Server tables and declares relationships.
 * Enterprise concept: a DbContext is the unit of work for one request and is
 * normally accessed through services rather than controllers.
 */
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Features.Auth;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Data;

public sealed class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(customer => customer.Email).IsUnique();
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.Price).HasPrecision(18, 2);
        });
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasOne(order => order.Customer).WithMany(customer => customer.Orders).HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(item => item.Order).WithMany(order => order.Items).HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product).WithMany(product => product.OrderItems).HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasIndex(cart => cart.CustomerId).IsUnique();
            entity.HasOne(cart => cart.Customer).WithMany().HasForeignKey(cart => cart.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(item => new { item.ShoppingCartId, item.ProductId }).IsUnique();
            entity.HasOne(item => item.ShoppingCart).WithMany(cart => cart.Items).HasForeignKey(item => item.ShoppingCartId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product).WithMany().HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasIndex(inventory => inventory.ProductId).IsUnique();
            entity.HasOne(inventory => inventory.Product).WithOne().HasForeignKey<Inventory>(inventory => inventory.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(payment => payment.IdempotencyKey).IsUnique();
            entity.HasOne(payment => payment.Order).WithMany().HasForeignKey(payment => payment.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
