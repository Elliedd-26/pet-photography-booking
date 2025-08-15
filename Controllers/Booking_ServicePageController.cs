using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.Models.ViewModels;

namespace PetPhotographyApp.Controllers
{
    public class Booking_ServicePageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Booking_ServicePageController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays all booking-service associations.
        /// Only accessible to logged-in users.
        /// </summary>
        /// <returns>
        /// View with list of Booking_Service entities or redirects to login.
        /// </returns>
        /// <example>
        /// GET: Booking_ServicePage
        /// </example>
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var list = await _context.Booking_Services
                .Include(b => b.Booking)
                .Include(b => b.Service)
                .ToListAsync();

            return View(list);
        }

        /// <summary>
        /// Renders the form to create a new booking-service link.
        /// Only accessible to logged-in users.
        /// </summary>
        /// <returns>
        /// View with dropdowns for Bookings and Services.
        /// </returns>
        /// <example>
        /// GET: Booking_ServicePage/Create
        /// </example>
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var viewModel = new BookingServiceViewModel
            {
                Bookings = _context.Bookings.Select(b => new SelectListItem
                {
                    Value = b.BookingId.ToString(),
                    Text = b.BookingId.ToString()
                }),
                Services = _context.Services.Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = s.Name
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles submission of new booking-service link.
        /// Only accessible to logged-in users.
        /// </summary>
        /// <param name="viewModel">BookingServiceViewModel containing selected BookingId and ServiceId</param>
        /// <returns>
        /// Redirects to Index on success, or redisplays form with validation errors.
        /// </returns>
        /// <example>
        /// POST: Booking_ServicePage/Create
        /// </example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingServiceViewModel viewModel)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var entity = new Booking_Service
                {
                    BookingId = viewModel.BookingId,
                    ServiceId = viewModel.ServiceId
                };

                _context.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if form is invalid
            viewModel.Bookings = _context.Bookings.Select(b => new SelectListItem
            {
                Value = b.BookingId.ToString(),
                Text = b.BookingId.ToString()
            });
            viewModel.Services = _context.Services.Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.Name
            });

            return View(viewModel);
        }

        /// <summary>
        /// Displays confirmation page to delete a booking-service link.
        /// Only accessible to Admin users.
        /// </summary>
        /// <param name="bookingId">ID of the booking</param>
        /// <param name="serviceId">ID of the service</param>
        /// <returns>
        /// View to confirm deletion or appropriate error/redirect.
        /// </returns>
        /// <example>
        /// GET: Booking_ServicePage/Delete?bookingId=1&serviceId=2
        /// </example>
        public async Task<IActionResult> Delete(int? bookingId, int? serviceId)
        {
            if (!IsAdmin()) return Unauthorized();

            if (bookingId == null || serviceId == null)
                return NotFound();

            var entity = await _context.Booking_Services
                .Include(b => b.Booking)
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.ServiceId == serviceId);

            if (entity == null) return NotFound();

            return View(entity);
        }

        /// <summary>
        /// Confirms and executes deletion of a booking-service link.
        /// Only accessible to Admin users.
        /// </summary>
        /// <param name="bookingId">ID of the booking</param>
        /// <param name="serviceId">ID of the service</param>
        /// <returns>
        /// Redirects to Index after deletion or returns Unauthorized.
        /// </returns>
        /// <example>
        /// POST: Booking_ServicePage/DeleteConfirmed
        /// </example>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int bookingId, int serviceId)
        {
            if (!IsAdmin()) return Unauthorized();

            var entity = await _context.Booking_Services.FindAsync(bookingId, serviceId);
            if (entity != null)
            {
                _context.Booking_Services.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Check if current user is logged in
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));
        }

        // Check if current user is Admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
