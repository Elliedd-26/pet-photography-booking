namespace PetPhotographyApp.Models.ViewModels
{
    public class BookingSummaryViewModel
    {
        public int BookingId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string PhotographerName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string? Location { get; set; }

        public string? PetName { get; set; }
        public int ServiceCount { get; set; }
    }
}
