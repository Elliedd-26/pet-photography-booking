namespace PetPhotographyApp.Models
{
    public class Booking_Service
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;
        public string Status { get; set; } = "Pending";
    }
}
