using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public DateTime BookingDate { get; set; }

        public string? Location { get; set; }

        public string? Notes { get; set; }

        public int OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;

        public int PetId { get; set; }
        public Pet Pet { get; set; } = null!;

        public int PhotographerId { get; set; }
        public Photographer Photographer { get; set; } = null!;
        public string Status { get; set; } = "Pending";

        public ICollection<Booking_Service> BookingServices { get; set; } = new List<Booking_Service>();
    }
}
