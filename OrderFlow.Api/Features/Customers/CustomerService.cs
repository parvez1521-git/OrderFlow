/*
 * Customer application service. It performs customer CRUD and enforces the
 * unique-email rule using the EF Core context injected by ASP.NET Core.
 * Enterprise concept: services are the application/business boundary between
 * controllers and persistence.
 */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;
namespace OrderFlow.Api.Features.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class CustomerService(OrderFlowDbContext db) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken ct) => await db.Customers.AsNoTracking().OrderBy(x => x.Id).Select(x => ToResponse(x)).ToListAsync(ct);
    public Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken ct) => db.Customers.AsNoTracking().Where(x => x.Id == id).Select(x => ToResponse(x)).SingleOrDefaultAsync(ct);
    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Customers.AnyAsync(x => x.Email == email, ct)) throw new BusinessRuleException("A customer with this email already exists.");
        var customer = new Customer { Name = request.Name.Trim(), Email = email };
        db.Customers.Add(customer); await db.SaveChangesAsync(ct); return ToResponse(customer);
    }
    public async Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct)
    {
        var customer = await db.Customers.FindAsync([id], ct); if (customer is null) return null;
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Customers.AnyAsync(x => x.Id != id && x.Email == email, ct)) throw new BusinessRuleException("A customer with this email already exists.");
        customer.Name = request.Name.Trim(); customer.Email = email; await db.SaveChangesAsync(ct); return ToResponse(customer);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var customer = await db.Customers.FindAsync([id], ct); if (customer is null) return false;
        if (await db.Orders.AnyAsync(x => x.CustomerId == id, ct)) throw new BusinessRuleException("A customer with orders cannot be deleted.");
        db.Customers.Remove(customer); await db.SaveChangesAsync(ct); return true;
    }
    private static CustomerResponse ToResponse(Customer customer) => new(customer.Id, customer.Name, customer.Email);
}
