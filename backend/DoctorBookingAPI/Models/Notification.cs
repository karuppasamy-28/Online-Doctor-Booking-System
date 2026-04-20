using System;
namespace DoctorBookingAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }   // patient id
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}