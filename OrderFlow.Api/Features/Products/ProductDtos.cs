/* Product request/response contracts. DTOs keep API data independent from the
 * EF Core entity and reject invalid product names or prices at the boundary. */
using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Features.Products;

public sealed record ProductResponse(int Id, string Name, decimal Price);
public sealed record CreateProductRequest([Required, StringLength(200)] string Name, [Range(typeof(decimal), "0.01", "9999999999999999.99")] decimal Price);
public sealed record UpdateProductRequest([Required, StringLength(200)] string Name, [Range(typeof(decimal), "0.01", "9999999999999999.99")] decimal Price);
