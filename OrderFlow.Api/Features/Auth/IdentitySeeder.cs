/* Development seed data for roles and local learning users. */
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Features.Auth;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<OrderFlowDbContext>();
        const string password = "ChangeMe123!";

        foreach (var role in new[] { "Admin", "OrderManager", "Customer" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        await EnsureUserAsync(users, db, "admin@oms.local", password, "Admin", null);
        await EnsureUserAsync(users, db, "ordermanager@oms.local", password, "OrderManager", null);

        var seededCustomer = await db.Customers.SingleOrDefaultAsync(x => x.Email == "customer@oms.local");
        if (seededCustomer is null)
        {
            seededCustomer = new Customer { Name = "Seeded Customer", Email = "customer@oms.local" };
            db.Customers.Add(seededCustomer);
            await db.SaveChangesAsync();
        }
        await EnsureUserAsync(users, db, "customer@oms.local", password, "Customer", seededCustomer.Id);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> users, OrderFlowDbContext db, string email, string password, string role, int? customerId)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, CustomerId = customerId };
            await users.CreateAsync(user, password);
        }
        if (!await users.IsInRoleAsync(user, role)) await users.AddToRoleAsync(user, role);
    }
}
