/* Shopping cart aggregate for Phase 4. */
namespace OrderFlow.Api.Models;

public sealed class ShoppingCart
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public sealed class CartItem
{
    public int Id { get; set; }
    public int ShoppingCartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public ShoppingCart ShoppingCart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
