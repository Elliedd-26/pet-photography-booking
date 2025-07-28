using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    public class Owner
    {
        [Key]
        public int OwnerId { get; set; }

       [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;    

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 digits")]
        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
