/* EF Core context for CustomerService's private database. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.CustomerService.Models;

namespace OrderFlow.CustomerService.Data;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().Property(x => x.Name).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Customer>().Property(x => x.Email).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
    }
}
