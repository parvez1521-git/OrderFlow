/* EF Core context for NotificationService's private database. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.NotificationService.Models;

namespace OrderFlow.NotificationService.Data;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>().Property(x => x.Recipient).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<Notification>().Property(x => x.Message).HasMaxLength(1000).IsRequired();
        modelBuilder.Entity<Notification>().Property(x => x.Status).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Notification>().Property(x => x.SourceEventId).HasMaxLength(100);
        modelBuilder.Entity<Notification>().HasIndex(x => x.SourceEventId).IsUnique().HasFilter("[SourceEventId] IS NOT NULL");
    }
}
