/* Shopping cart request/response contracts. */
using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Features.Cart;

public sealed record CartItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public sealed record CartResponse(int CustomerId, decimal Total, IReadOnlyCollection<CartItemResponse> Items);
public sealed record CartItemRequest([Range(1, int.MaxValue)] int ProductId, [Range(1, int.MaxValue)] int Quantity);
