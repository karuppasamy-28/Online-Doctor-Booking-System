using Microsoft.AspNetCore.Mvc;
using DoctorBookingAPI.Data;
using DoctorBookingAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoctorController(AppDbContext context)
    {
        _context = context;
    }

    // ➕ Add Doctor
   [HttpPost("add")]
public IActionResult AddDoctor([FromBody] Doctor doctor)
{
    // ✅ If admin adds doctor → auto approve
    doctor.Status = "APPROVED";

    _context.Doctors.Add(doctor);
    _context.SaveChanges();

    return Ok("Doctor added successfully");
}

    // 📋 Get All Doctors
[HttpGet("all")]
public IActionResult GetDoctors()
{
    return Ok(_context.Doctors
        .Where(d => d.Status == "APPROVED")
        .ToList());
}
    [HttpGet("search")]
public IActionResult SearchDoctors(string specialization)
{
    var doctors = _context.Doctors
        .Where(d => d.Specialization.Contains(specialization))
        .ToList();

    return Ok(doctors);
}
[HttpGet("{id}")]
public IActionResult GetDoctorById(int id)
{
    var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);

    if (doctor == null)
        return NotFound("Doctor not found");

    return Ok(doctor);
}

[HttpPost("login")]
public IActionResult DoctorLogin([FromBody] Doctor login)
{
    var doctor = _context.Doctors
        .FirstOrDefault(d => d.Email == login.Email && d.Password == login.Password);

    if (doctor == null)
        return Unauthorized("Invalid credentials");

    if (doctor.Status != "APPROVED")
        return Unauthorized("Wait for admin approval ❌");

    return Ok(new {
        id = doctor.DoctorId,
        name = doctor.Name
    });
}

[HttpGet("appointments/{doctorId}")]
public IActionResult GetDoctorAppointments(int doctorId)
{
    var data = _context.Appointments
        .Where(a => a.DoctorId == doctorId)
        .ToList();

    return Ok(data);
}


[HttpPut("approve/{id}")]
public IActionResult ApproveDoctor(int id)
{
    var doc = _context.Doctors.Find(id);

    if (doc == null) return NotFound();

    doc.Status = "APPROVED";
    _context.SaveChanges();

    return Ok();
}

[HttpPut("remove/{id}")]
public IActionResult RemoveDoctor(int id)
{
    var doc = _context.Doctors.Find(id);

    if (doc == null)
        return NotFound("Doctor not found");

    // ✅ Step 1: Mark doctor as removed
    doc.Status = "REMOVED";

    // ✅ Step 2: Cancel all upcoming appointments
    var appointments = _context.Appointments
        .Where(a => a.DoctorId == id && a.Status != "CANCELLED")
        .ToList();

    foreach (var appt in appointments)
    {
        appt.Status = "CANCELLED";
    }

    _context.SaveChanges();

    return Ok("Doctor removed and appointments cancelled");
}
[HttpPost("register")]
public IActionResult Register([FromBody] Doctor doctor)
{
    doctor.Status = "PENDING";   // 🔥 important

    _context.Doctors.Add(doctor);
    _context.SaveChanges();

    return Ok("Doctor registered. Waiting for admin approval");
}
// 🔥 ADMIN VIEW ALL DOCTORS (PENDING + APPROVED + REMOVED)
[HttpGet("admin")]
public IActionResult GetAllDoctorsForAdmin()
{
    return Ok(_context.Doctors.ToList());
}


}