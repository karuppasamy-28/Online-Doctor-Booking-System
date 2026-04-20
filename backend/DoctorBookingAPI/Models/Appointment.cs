using System;
namespace DoctorBookingAPI.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
    }
}