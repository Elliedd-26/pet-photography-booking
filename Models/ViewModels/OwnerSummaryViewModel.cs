namespace PetPhotographyApp.Models.ViewModels
{
    public class OwnerSummaryViewModel
    {
        public int OwnerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public int PetCount { get; set; }
    }
}
