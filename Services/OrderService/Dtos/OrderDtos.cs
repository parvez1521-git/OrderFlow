/* Order request contracts for this microservice. */
namespace OrderFlow.OrderService.Dtos;

public sealed record CreateOrderRequest(int CustomerId, IReadOnlyCollection<CreateOrderItemRequest> Items);
public sealed record CreateOrderItemRequest(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record OrderResponse(int Id, int CustomerId, DateTime OrderDateUtc, string Status, decimal Total, IReadOnlyCollection<OrderItemResponse> Items);
public sealed record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record CheckoutRequest(int CustomerId, string CustomerEmail, string PaymentMode, string IdempotencyKey, IReadOnlyCollection<CheckoutItemRequest> Items);
public sealed record CheckoutItemRequest(int ProductId, int Quantity);
public sealed record CheckoutResponse(int OrderId, string OrderStatus, string PaymentStatus, decimal Total, string Message);
