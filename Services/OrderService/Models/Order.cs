/* Order aggregate owned by the Order microservice. */
namespace OrderFlow.OrderService.Models;

public sealed class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDateUtc { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderStatus { Pending, Confirmed, Cancelled, PaymentFailed }

public sealed class CheckoutSagaState
{
    public int Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
