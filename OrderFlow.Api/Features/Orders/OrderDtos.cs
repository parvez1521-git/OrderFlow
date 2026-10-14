/* Order request/response contracts. Client requests send product IDs and
 * quantities only; the backend, not the client, determines UnitPrice. */
using System.ComponentModel.DataAnnotations;
using OrderFlow.Api.Models;
namespace OrderFlow.Api.Features.Orders;

public sealed record OrderItemResponse(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record OrderResponse(int Id, int CustomerId, DateTime OrderDate, OrderStatus Status, decimal Total, IReadOnlyCollection<OrderItemResponse> Items);
public sealed record OrderItemRequest([Range(1, int.MaxValue)] int ProductId, [Range(1, int.MaxValue)] int Quantity);
public sealed record CreateOrderRequest([Range(1, int.MaxValue)] int CustomerId, [Required, MinLength(1)] IReadOnlyCollection<OrderItemRequest> Items);
public sealed record UpdateOrderRequest([Range(1, int.MaxValue)] int CustomerId, [Required, MinLength(1)] IReadOnlyCollection<OrderItemRequest> Items);
