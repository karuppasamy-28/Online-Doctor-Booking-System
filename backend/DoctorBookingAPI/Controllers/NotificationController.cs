using Microsoft.AspNetCore.Mvc;
using DoctorBookingAPI.Data;
using DoctorBookingAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationController(AppDbContext context)
    {
        _context = context;
    }

    // 🔔 GET NOTIFICATIONS
    [HttpGet("{userId}")]
    public IActionResult GetNotifications(int userId)
    {
        var data = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return Ok(data);
    }

    // ✔ MARK AS READ
    [HttpPut("mark-read/{id}")]
    public IActionResult MarkAsRead(int id)
    {
        var notif = _context.Notifications.Find(id);

        if (notif == null)
            return NotFound();

        notif.IsRead = true;
        _context.SaveChanges();

        return Ok();
    }
}