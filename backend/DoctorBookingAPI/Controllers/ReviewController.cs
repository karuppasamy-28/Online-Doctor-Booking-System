using Microsoft.AspNetCore.Mvc;
using DoctorBookingAPI.Data;
using DoctorBookingAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    // ⭐ ADD REVIEW
    [HttpPost("add")]
    public IActionResult AddReview(Review review)
    {
        // ❗ allow only completed appointment
        var appt = _context.Appointments
            .FirstOrDefault(a => a.AppointmentId == review.AppointmentId);

        if (appt == null || appt.Status != "COMPLETED")
            return BadRequest("Cannot review before completion");

        // ❗ prevent duplicate review
        var exists = _context.Reviews.Any(r =>
            r.AppointmentId == review.AppointmentId &&
            r.PatientId == review.PatientId);

        if (exists)
            return BadRequest("Already reviewed");

        _context.Reviews.Add(review);
        _context.SaveChanges();

        return Ok();
    }

   [HttpGet("doctor/{doctorId}")]
public IActionResult GetReviews(int doctorId)
{
    var data = (from r in _context.Reviews
                join u in _context.Users
                on r.PatientId equals u.Id
                where r.DoctorId == doctorId
                orderby r.CreatedAt descending
                select new
                {
                    r.Rating,
                    r.Comment,
                    PatientName = u.Name,   // ✅ patient name
                    r.CreatedAt             // ✅ date
                }).ToList();

    return Ok(data);
}

    // ⭐ AVERAGE
    [HttpGet("average/{doctorId}")]
    public IActionResult GetAverage(int doctorId)
    {
        var avg = _context.Reviews
            .Where(r => r.DoctorId == doctorId)
            .Average(r => (double?)r.Rating) ?? 0;

        return Ok(avg);
    }
}