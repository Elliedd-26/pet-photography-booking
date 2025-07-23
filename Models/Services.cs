using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public ICollection<Booking_Service> BookingServices { get; set; } = new List<Booking_Service>();
    }
}
