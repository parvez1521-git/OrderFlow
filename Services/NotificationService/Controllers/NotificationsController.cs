/* Controller for notification endpoints. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.NotificationService.Data;
using OrderFlow.NotificationService.Dtos;
using OrderFlow.NotificationService.Models;

namespace OrderFlow.NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(NotificationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Notification>> GetAll() => await db.Notifications.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Notification>> GetById(int id) => await db.Notifications.FindAsync(id) is { } notification ? Ok(notification) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Notification>> Create(NotificationRequest request)
    {
        var notification = new Notification { OrderId = request.OrderId, Recipient = request.Recipient.Trim(), Message = request.Message.Trim(), Status = "Queued", CreatedAtUtc = DateTime.UtcNow };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { notification.Id }, notification);
    }

    [HttpPost("{id:int}/mark-sent")]
    public async Task<ActionResult<Notification>> MarkSent(int id)
    {
        var notification = await db.Notifications.FindAsync(id);
        if (notification is null) return NotFound();
        notification.Status = "Sent";
        await db.SaveChangesAsync();
        return Ok(notification);
    }
}
