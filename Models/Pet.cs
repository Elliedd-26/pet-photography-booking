using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetPhotographyApp.Models
{
    public class Pet
    {
        [Key]
        public int PetId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Species is required.")]
        public string Species { get; set; } = string.Empty;

        public string? Breed { get; set; }

        [Range(0, 50, ErrorMessage = "Age must be between 0 and 50.")]
        public int? Age { get; set; }

        public string? Color { get; set; }
        public string? SpecialNotes { get; set; }
        public string? Description { get; set; }

        public string? PhotoPath { get; set; }  // optional photo path for profile picture

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int OwnerId { get; set; }

        [ValidateNever]
        public Owner Owner { get; set; } = null!;

        [ValidateNever]
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
