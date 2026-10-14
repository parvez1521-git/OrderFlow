/* Controller for order endpoints. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrderService.Data;
using OrderFlow.OrderService.Dtos;
using OrderFlow.OrderService.Models;

namespace OrderFlow.OrderService.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(OrderDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<OrderResponse>> GetAll()
    {
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.Id).ToListAsync();
        return orders.Select(ToResponse).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id) => await db.Orders.AsNoTracking().Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id) is { } order ? Ok(ToResponse(order)) : NotFound();

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var order = new Order { CustomerId = request.CustomerId, OrderDateUtc = DateTime.UtcNow, Status = OrderStatus.Pending };
        foreach (var item in request.Items) order.Items.Add(new OrderItem { ProductId = item.ProductId, ProductName = item.ProductName.Trim(), Quantity = item.Quantity, UnitPrice = item.UnitPrice });
        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { order.Id }, ToResponse(order));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderResponse>> Update(int id, CreateOrderRequest request)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        order.CustomerId = request.CustomerId;
        order.Items.Clear();
        foreach (var item in request.Items) order.Items.Add(new OrderItem { ProductId = item.ProductId, ProductName = item.ProductName.Trim(), Quantity = item.Quantity, UnitPrice = item.UnitPrice });
        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        await db.SaveChangesAsync();
        return Ok(ToResponse(order));
    }

    [HttpPost("{id:int}/confirm")]
    public Task<ActionResult<OrderResponse>> Confirm(int id) => SetStatusAsync(id, OrderStatus.Confirmed);

    [HttpPost("{id:int}/cancel")]
    public Task<ActionResult<OrderResponse>> Cancel(int id) => SetStatusAsync(id, OrderStatus.Cancelled);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult<OrderResponse>> SetStatusAsync(int id, OrderStatus status)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        order.Status = status;
        await db.SaveChangesAsync();
        return Ok(ToResponse(order));
    }

    private static OrderResponse ToResponse(Order order) =>
        new(order.Id, order.CustomerId, order.OrderDateUtc, order.Status.ToString(), order.Total,
            order.Items.Select(item => new OrderItemResponse(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice)).ToList());
}
