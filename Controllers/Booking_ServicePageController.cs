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

        // GET: Booking_ServicePage
        // Show all booking-service associations (only for logged-in users)
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var list = await _context.Booking_Services
                .Include(b => b.Booking)
                .Include(b => b.Service)
                .ToListAsync();

            return View(list);
        }

        // GET: Booking_ServicePage/Create
        // Render form to create a new booking-service link
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

        // POST: Booking_ServicePage/Create
        // Handle form submission to create booking-service link
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

        // GET: Booking_ServicePage/Delete
        // Only Admin can delete booking-service link
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

        // POST: Booking_ServicePage/DeleteConfirmed
        // Execute deletion (only for Admin)
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
