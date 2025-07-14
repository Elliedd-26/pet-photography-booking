using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    /// <summary>
    /// Represents a pet owner who books photography sessions
    /// </summary>
    public class Owner
    {
        // Primary key
        public int Id { get; set; }
        
        // Owner's full name (required, max 100 characters)
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        // Contact email (required, valid email format)
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        // Phone number (optional, max 20 characters)
        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }
        
        // Home address (optional, max 200 characters)
        [StringLength(200)]
        public string? Address { get; set; }
        
        // Account creation timestamp
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties - relationships to other entities
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}