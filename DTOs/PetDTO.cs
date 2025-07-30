namespace PetPhotographyApp.DTOs
{
    public class PetDTO
    {
        public int PetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public int? Age { get; set; }
        public string OwnerName { get; set; } = string.Empty;
    }
}
