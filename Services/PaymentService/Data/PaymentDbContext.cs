/* EF Core context for PaymentService's private database. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.PaymentService.Models;

namespace OrderFlow.PaymentService.Data;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        modelBuilder.Entity<Payment>().HasIndex(x => x.IdempotencyKey).IsUnique();
    }
}
