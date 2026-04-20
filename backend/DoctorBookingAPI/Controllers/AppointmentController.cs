using Microsoft.AspNetCore.Mvc;
using DoctorBookingAPI.Data;
using DoctorBookingAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentController(AppDbContext context)
    {
        _context = context;
    }

  

    // 📋 View All Appointments
   [HttpGet("all")]
public IActionResult GetAppointments()
{
    var data = (from a in _context.Appointments
                join u in _context.Users
                on a.PatientId equals u.Id
                join d in _context.Doctors
                on a.DoctorId equals d.DoctorId
                select new
                {
                    a.AppointmentId,
                    PatientName = u.Name,   // ✅ patient name
                    DoctorName = d.Name,    // ✅ doctor name
                    a.Date,
                    a.TimeSlot,
                    a.Status
                }).ToList();

    return Ok(data);
}

 [HttpGet("patient/{id}")]
public IActionResult GetAppointmentsByPatient(int id)
{
    var data = (from a in _context.Appointments
                join d in _context.Doctors
                on a.DoctorId equals d.DoctorId
                where a.PatientId == id
                select new
                {
                    a.AppointmentId,
                    a.Date,
                    a.TimeSlot,
                    a.Status,
                    a.DoctorId,  
                    DoctorName = d.Name   // 🔥 IMPORTANT
                }).ToList();

    return Ok(data);
}

[HttpPut("cancel/{id}")]
public IActionResult CancelAppointment(int id)
{
    var appt = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);

    if (appt == null)
        return NotFound("Appointment not found");

    appt.Status = "CANCELLED";
    _context.SaveChanges();

    return Ok("Appointment cancelled successfully");
}

[HttpPut("update-status/{id}")]
public IActionResult UpdateStatus(int id, string status)
{
    var appt = _context.Appointments
        .FirstOrDefault(a => a.AppointmentId == id);

    if (appt == null)
        return NotFound("Appointment not found");

    appt.Status = status; // APPROVED / REJECTED
    _context.SaveChanges();

    return Ok("Status updated");
}

[HttpGet("doctor/{id}")]
public IActionResult GetByDoctor(int id)
{
    var data = (from a in _context.Appointments
                join u in _context.Users
                on a.PatientId equals u.Id
                where a.DoctorId == id
                select new
                {
                    a.AppointmentId,
                    a.Date,
                    a.TimeSlot,
                    a.Status,
                    PatientName = u.Name
                }).ToList();

    return Ok(data);
}

[HttpPut("accept/{id}")]
public IActionResult Accept(int id)
{
    var appt = _context.Appointments.Find(id);

    if (appt == null) return NotFound();

    // ✅ Step 1: Update status
    appt.Status = "ACCEPTED";

    // 🔔 Step 2: Create notification
    var notification = new Notification
    {
        UserId = appt.PatientId,
        Message = "Your appointment has been ACCEPTED by doctor"
    };

    _context.Notifications.Add(notification);

    _context.SaveChanges();

    return Ok();
}

[HttpPut("reject/{id}")]
public IActionResult Reject(int id)
{
    var appt = _context.Appointments.Find(id);

    if (appt == null) return NotFound();

    // ❗ Step 1: Update status
    appt.Status = "REJECTED";

    // 🔔 Step 2: Create notification
    var notification = new Notification
    {
        UserId = appt.PatientId,
        Message = "Your appointment has been REJECTED by doctor"
    };

    _context.Notifications.Add(notification);

    _context.SaveChanges();

    return Ok();
}

[HttpPost("book")]
public IActionResult Book(Appointment appt)
{
    var exists = _context.Appointments.Any(a =>
        a.DoctorId == appt.DoctorId &&   // ✅ IMPORTANT
        a.Date.Date == appt.Date.Date && // ✅ DATE ONLY
        a.TimeSlot == appt.TimeSlot &&
        a.Status != "CANCELLED"
    );

    if (exists)
    {
        return BadRequest("Slot already booked ❌");
    }

    appt.Status = "BOOKED";
    _context.Appointments.Add(appt);
    _context.SaveChanges();

    return Ok();
}

[HttpPut("complete/{id}")]
public IActionResult Complete(int id)
{
    var appt = _context.Appointments.Find(id);

    if (appt == null) return NotFound();

    // ✅ Update status
    appt.Status = "COMPLETED";

    // 🔔 Notification to patient
    var notification = new Notification
    {
        UserId = appt.PatientId,
        Message = "Your appointment has been COMPLETED by doctor"
    };

    _context.Notifications.Add(notification);

    _context.SaveChanges();

    return Ok();
}
}