
namespace PetPhotographyApp.DTOs
{
    public class ServiceDTO
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}
