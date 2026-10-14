/* Product request/response contracts for this microservice. */
namespace OrderFlow.ProductService.Dtos;

public sealed record ProductRequest(string Name, decimal Price);
