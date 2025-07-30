using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetPhotographyApp.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "Booking";

        public bool IsRead { get; set; } = false;

    
        public DateTime SentAt { get; set; } = DateTime.Now;


        public int RecipientOwnerId { get; set; }

        [ValidateNever]
        public Owner RecipientOwner { get; set; } = null!;
    }
}
