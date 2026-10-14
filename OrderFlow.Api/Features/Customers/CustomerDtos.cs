/* Customer request/response contracts. Validation attributes describe input
 * rules, while records make small immutable API payloads concise. */
using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Features.Customers;

public sealed record CustomerResponse(int Id, string Name, string Email);
public sealed record CreateCustomerRequest([Required, StringLength(200)] string Name, [Required, EmailAddress, StringLength(320)] string Email);
public sealed record UpdateCustomerRequest([Required, StringLength(200)] string Name, [Required, EmailAddress, StringLength(320)] string Email);
