namespace PetPhotographyApp.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Location { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;

        public int PetId { get; set; }
        public string PetName { get; set; } = null!;

        public int PhotographerId { get; set; }
        public string PhotographerName { get; set; } = null!;

        public List<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();
    }
}