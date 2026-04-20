using Microsoft.AspNetCore.Mvc;
using DoctorBookingAPI.Data;
using DoctorBookingAPI.Models;
using System.Linq;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User Registered Successfully");
    }
    [HttpPost("login")]
public IActionResult Login([FromBody] User loginUser)
{
    var user = _context.Users
        .FirstOrDefault(u => u.Email == loginUser.Email && u.Password == loginUser.Password);

    if (user == null)
    {
        return Unauthorized("Invalid email or password");
    }

    return Ok(new
    {
        message = "Login successful",
        user.Id,
        user.Name,
        user.Email,
        user.Role
    });
}
}