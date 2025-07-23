using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    public class Pet
    {
        [Key]
        public int PetId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Species { get; set; }
        public string? Breed { get; set; }
        public int? Age { get; set; }
        public string? Color { get; set; }
        public string? SpecialNotes { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
