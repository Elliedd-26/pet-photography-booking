namespace PetPhotographyApp.DTOs
{
    public class OwnerDTO
    {
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<PetDTO> Pets { get; set; } = new List<PetDTO>();
    }
}