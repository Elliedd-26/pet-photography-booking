namespace PetPhotographyApp.DTOs
{
    public class OwnerDTO
    {
        public int OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public List<PetDTO> Pets { get; set; } = new List<PetDTO>();
    }
}