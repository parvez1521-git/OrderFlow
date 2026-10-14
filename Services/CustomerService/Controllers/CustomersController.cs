/* Controller for customer endpoints. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.CustomerService.Data;
using OrderFlow.CustomerService.Dtos;
using OrderFlow.CustomerService.Models;

namespace OrderFlow.CustomerService.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(CustomerDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Customer>> GetAll() => await db.Customers.AsNoTracking().OrderBy(x => x.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id) => await db.Customers.FindAsync(id) is { } customer ? Ok(customer) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(CustomerRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Customers.AnyAsync(x => x.Email == email)) return Conflict("A customer with this email already exists.");
        var customer = new Customer { Name = request.Name.Trim(), Email = email };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Customer>> Update(int id, CustomerRequest request)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        customer.Name = request.Name.Trim();
        customer.Email = request.Email.Trim().ToLowerInvariant();
        await db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
