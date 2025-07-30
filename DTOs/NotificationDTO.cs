namespace PetPhotographyApp.DTOs
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Booking";
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; }

        public int RecipientOwnerId { get; set; }
        public string RecipientOwnerName { get; set; } = string.Empty;
    }
}
