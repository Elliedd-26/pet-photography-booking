using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    /// <summary>
    /// Represents a pet that will be photographed
    /// </summary>
    public class Pet
    {
        // Primary key
        public int Id { get; set; }
        
        // Pet's name (required, max 100 characters)
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        // Type of animal (required, e.g., Dog, Cat, Bird)
        [Required]
        [StringLength(50)]
        public string Species { get; set; } = string.Empty;
        
        // Specific breed (optional, max 100 characters)
        [StringLength(100)]
        public string? Breed { get; set; }
        
        // Pet's age in years
        public int Age { get; set; }
        
        // Pet's color/markings (optional, max 20 characters)
        [StringLength(20)]
        public string? Color { get; set; }
        
        // Special notes for photographer (behavioral notes, etc.)
        [StringLength(500)]
        public string? SpecialNotes { get; set; }
        
        // Record creation timestamp
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign key - links to the owner
        public int OwnerId { get; set; }
        
        // Navigation property - reference to the owner
        public virtual Owner Owner { get; set; } = null!;
    }
}