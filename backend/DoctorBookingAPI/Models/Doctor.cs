namespace DoctorBookingAPI.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public int Experience { get; set; }
        public decimal Fees { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; } = "PENDING";
        public string ImageUrl { get; set; }
    }
}