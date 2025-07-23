public class PhotographerDTO
{
    public int PhotographerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Specialty { get; set; }
    public bool IsAvailable { get; set; }
}
