using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    /// <summary>
    /// Represents notifications sent to pet owners
    /// Simplified version - no booking/photographer links for now
    /// </summary>
    public class Notification
    {
        // Primary key
        public int Id { get; set; }
        
        // Notification content (required, max 500 characters)
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        // Type of notification (e.g., Booking, Reminder, Confirmation, Welcome)
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;
        
        // Whether the notification has been read by the owner
        public bool IsRead { get; set; } = false;
        
        // When the notification was created
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign key - links to the owner who receives this notification
        public int OwnerId { get; set; }
        
        // Navigation property - reference to the owner
        public virtual Owner Owner { get; set; } = null!;
    }
}