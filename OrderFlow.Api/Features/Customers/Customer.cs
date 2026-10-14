/* Customer persistence model. EF Core uses it to represent the Customers table.
 * Enterprise concept: entities model stored data and are not returned directly
 * from the API; DTOs protect the API contract from persistence changes. */
namespace OrderFlow.Api.Models;

public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
