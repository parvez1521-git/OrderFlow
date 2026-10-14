/* Customer request contracts for this microservice. */
namespace OrderFlow.CustomerService.Dtos;

public sealed record CustomerRequest(string Name, string Email);
