namespace PetPhotographyApp.Models.ViewModels
{
    public class BookingFormViewModel
    {
        // Form Fields
        public int? BookingId { get; set; } 
        public DateTime BookingDate { get; set; }
        public string? Location { get; set; }

        public int OwnerId { get; set; }
         public int PetId { get; set; }
        public int PhotographerId { get; set; }
        public string? Notes { get; set; } 

        // Multi-select Services
        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        // Dropdown/population sources
        public IEnumerable<Owner> Owners { get; set; } = new List<Owner>();
        public IEnumerable<Pet> Pets { get; set; } = new List<Pet>();
        public IEnumerable<Photographer> Photographers { get; set; } = new List<Photographer>();
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
    }
}
