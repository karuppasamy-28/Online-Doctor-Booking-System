using System;
namespace DoctorBookingAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public int AppointmentId { get; set; } // 🔥 important

        public int Rating { get; set; } // 1–5
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}