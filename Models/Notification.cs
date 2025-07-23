using System.ComponentModel.DataAnnotations;

namespace PetPhotographyApp.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "Booking";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;
    }
}
