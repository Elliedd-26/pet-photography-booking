using PetPhotographyApp.DTOs;

namespace PetPhotographyApp.Models.ViewModels
{
    public class BookingDetailsViewModel
    {
        public required BookingDTO Booking { get; set; }
        public IEnumerable<ServiceDTO>? Services { get; set; }
    }
}
