using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    public class Photographer
    {
        [Key]
        public int PhotographerId { get; set; }

        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Specialty { get; set; }

        public bool IsAvailable { get; set; } = true;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
