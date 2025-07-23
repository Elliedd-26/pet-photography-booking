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

        public async Task<IActionResult> Index()
        {
            var list = await _context.Booking_Services
                .Include(b => b.Booking)
                .Include(b => b.Service)
                .ToListAsync();

            return View(list);
        }

        public IActionResult Create()
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingServiceViewModel viewModel)
        {
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

        public async Task<IActionResult> Delete(int? bookingId, int? serviceId)
        {
            if (bookingId == null || serviceId == null)
                return NotFound();

            var entity = await _context.Booking_Services
                .Include(b => b.Booking)
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.ServiceId == serviceId);

            if (entity == null) return NotFound();

            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int bookingId, int serviceId)
        {
            var entity = await _context.Booking_Services.FindAsync(bookingId, serviceId);
            if (entity != null)
            {
                _context.Booking_Services.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
