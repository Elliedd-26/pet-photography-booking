namespace PetPhotographyApp.DTOs
{
    public class PetDTO
    {
        public int PetId { get; set; }
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string? Breed { get; set; }
        public int? Age { get; set; }
        public string? Description { get; set; }
        public string OwnerName { get; set; } = null!;
    }
}