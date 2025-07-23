using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetPhotographyApp.Models.ViewModels
{
    public class BookingServiceViewModel
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public int ServiceCount { get; set; }

        public IEnumerable<SelectListItem>? Bookings { get; set; }
        public IEnumerable<SelectListItem>? Services { get; set; }
    }
}
