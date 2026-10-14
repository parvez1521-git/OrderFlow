/* HTTP endpoint adapter for customer CRUD. Controllers translate HTTP into
 * service calls and keep business rules in the service layer. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderFlow.Api.Features.Auth;
using OrderFlow.Api.Services;
namespace OrderFlow.Api.Features.Customers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService customers) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet] public Task<IReadOnlyList<CustomerResponse>> GetAll(CancellationToken ct) => customers.GetAllAsync(ct);
    [Authorize]
    [HttpGet("me")] public async Task<ActionResult<CustomerResponse>> Me(CancellationToken ct) => (await customers.GetByIdAsync(User.RequiredCustomerId(), ct)) is { } customer ? Ok(customer) : NotFound();
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")] public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken ct) => (await customers.GetByIdAsync(id, ct)) is { } customer ? Ok(customer) : NotFound();
    [Authorize(Roles = "Admin")]
    [HttpPost] public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        try { var customer = await customers.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { customer.Id }, customer); }
        catch (BusinessRuleException ex) { return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict }); }
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")] public async Task<ActionResult<CustomerResponse>> Update(int id, UpdateCustomerRequest request, CancellationToken ct)
    {
        try { return (await customers.UpdateAsync(id, request, ct)) is { } customer ? Ok(customer) : NotFound(); }
        catch (BusinessRuleException ex) { return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict }); }
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try { return await customers.DeleteAsync(id, ct) ? NoContent() : NotFound(); }
        catch (BusinessRuleException ex) { return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict }); }
    }
}
