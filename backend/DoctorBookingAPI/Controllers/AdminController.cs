using Microsoft.AspNetCore.Mvc;
using DoctorBookingAPI.Data;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // 🔐 Admin Login
[HttpPost("login")]
public IActionResult Login([FromBody] LoginModel model)
{
    if (model.Email == "admin" && model.Password == "admin123")
    {
        return Ok(new
        {
            id = 0,
            name = "Admin"
        });
    }

    return Unauthorized("Invalid admin credentials");
}

    // 👤 View All Users (STEP 2)
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok(_context.Users.ToList());
    }

    // 🧑‍⚕️ View All Doctors (STEP 3)
    [HttpGet("doctors")]
    public IActionResult GetDoctors()
    {
        return Ok(_context.Doctors.ToList());
    }
    [HttpPut("approve-doctor/{id}")]
public IActionResult ApproveDoctor(int id, [FromServices] AppDbContext _context)
{
    var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);

    if (doctor == null)
        return NotFound("Doctor not found");

    doctor.Status = "APPROVED";
    _context.SaveChanges();

    return Ok("Doctor approved");
}
[HttpGet("appointments")]
public IActionResult GetAppointments([FromServices] AppDbContext _context)
{
    return Ok(_context.Appointments.ToList());
}
}