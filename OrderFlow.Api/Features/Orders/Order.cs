/* Order persistence model.
 * Phase 6/7 expands order states so checkout and mock payment can show
 * confirmed, cancelled, and payment-failed outcomes.
 */
namespace OrderFlow.Api.Models;

public sealed class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderStatus { Pending, Confirmed, Cancelled, PaymentFailed }
