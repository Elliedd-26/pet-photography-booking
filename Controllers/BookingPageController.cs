using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.Models.ViewModels;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// MVC Controller for managing bookings (Admin and User role support)
    /// </summary>
    public class BookingPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingPageController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        /// <summary>
        /// Displays a list of all bookings (admin and user access).
        /// </summary>
        /// <returns>View with a list of BookingSummaryViewModel. Redirects to login if user is not logged in.</returns>
        /// <example>GET: BookingPage</example>  
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var bookings = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Select(b => new BookingSummaryViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    Location = b.Location,
                    OwnerName = b.Owner.Name,
                    PetName = b.Pet.Name,
                    PhotographerName = b.Photographer.Name,
                    ServiceCount = _context.Booking_Services.Count(bs => bs.BookingId == b.BookingId)
                })
                .ToListAsync();

            return View(bookings);
        }

        /// <summary>
        /// Displays detailed information for a specific booking.
        /// </summary>
        /// <param name="id">The ID of the booking</param>
        /// <returns>BookingDetailsViewModel or 404 if not found. Redirects to login if not logged in.</returns>
        /// <example>GET: BookingPage/Details/5</example>
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var booking = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            var viewModel = new BookingDetailsViewModel
            {
                Booking = new DTOs.BookingDTO
                {
                    BookingId = booking.BookingId,
                    BookingDate = booking.BookingDate,
                    Location = booking.Location,
                    OwnerId = booking.OwnerId,
                    OwnerName = booking.Owner.Name,
                    PetId = booking.PetId,
                    PetName = booking.Pet.Name,
                    PhotographerId = booking.PhotographerId,
                    PhotographerName = booking.Photographer.Name,
                    Services = booking.BookingServices.Select(bs => new DTOs.ServiceDTO
                    {
                        ServiceId = bs.ServiceId,
                        Name = bs.Service.Name,
                        Price = bs.Service.Price
                    }).ToList()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the booking creation form.
        /// </summary>
        /// <returns>View with BookingFormViewModel. Redirects to login if not logged in.</returns>
        /// <example>GET: BookingPage/Create</example>
        public async Task<IActionResult> Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var viewModel = new BookingFormViewModel
            {
                BookingDate = DateTime.Today,
                Owners = await _context.Owners.OrderBy(o => o.Name).ToListAsync(),
                Pets = await _context.Pets.Include(p => p.Owner).OrderBy(p => p.Name).ToListAsync(),
                Photographers = await _context.Photographers.Where(p => p.IsAvailable).OrderBy(p => p.Name).ToListAsync(),
                Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles the creation of a new booking with selected services.
        /// </summary>
        /// <param name="model">The form model containing booking and selected services</param>
        /// <returns>Redirects to Index on success. Redisplays form with validation errors on failure.</returns>
        /// <example>POST: BookingPage/Create</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingFormViewModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            if (!ModelState.IsValid)
            {
                await RepopulateFormData(model);
                return View(model);
            }

            if (model.OwnerId == 0 || model.PetId == 0 || model.PhotographerId == 0)
            {
                ModelState.AddModelError("", "Please select all required fields.");
                await RepopulateFormData(model);
                return View(model);
            }

            var booking = new Booking
            {
                BookingDate = model.BookingDate,
                Location = model.Location,
                OwnerId = model.OwnerId,
                PetId = model.PetId,
                PhotographerId = model.PhotographerId,
                Status = "Pending",
                Notes = model.Notes
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            if (model.SelectedServiceIds != null)
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    _context.Booking_Services.Add(new Booking_Service
                    {
                        BookingId = booking.BookingId,
                        ServiceId = serviceId,
                        Status = "Pending"
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Loads the edit form for an existing booking (Admin only).
        /// </summary>
        /// <param name="id">The ID of the booking to edit</param>
        /// <returns>View with BookingFormViewModel or 404 if booking not found</returns>
        /// <example>GET: BookingPage/Edit/5</example>
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            var model = new BookingFormViewModel
            {
                BookingId = booking.BookingId,
                BookingDate = booking.BookingDate,
                Location = booking.Location,
                OwnerId = booking.OwnerId,
                PetId = booking.PetId,
                PhotographerId = booking.PhotographerId,
                Notes = booking.Notes,
                SelectedServiceIds = booking.BookingServices.Select(bs => bs.ServiceId).ToList()
            };

            await RepopulateFormData(model);
            return View(model);
        }

        /// <summary>
        /// Saves changes made to a booking (Admin only).
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="model">Updated form model</param>
        /// <returns>Redirects to Index on success. Redisplays form or returns error on failure.</returns>
        /// <example>POST: BookingPage/Edit/5</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingFormViewModel model)
        {
            if (!IsAdmin()) return Unauthorized();

            if (id != model.BookingId) return BadRequest();

            if (!ModelState.IsValid)
            {
                await RepopulateFormData(model);
                return View(model);
            }

            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            booking.BookingDate = model.BookingDate;
            booking.Location = model.Location;
            booking.OwnerId = model.OwnerId;
            booking.PetId = model.PetId;
            booking.PhotographerId = model.PhotographerId;
            booking.Notes = model.Notes;

            _context.Booking_Services.RemoveRange(booking.BookingServices);

            if (model.SelectedServiceIds != null)
            {
                booking.BookingServices = model.SelectedServiceIds.Select(serviceId => new Booking_Service
                {
                    BookingId = booking.BookingId,
                    ServiceId = serviceId,
                    Status = "Pending"
                }).ToList();
            }

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays confirmation page for deleting a booking (Admin only).
        /// </summary>
        /// <param name="id">The ID of the booking</param>
        /// <returns>View with booking details for confirmation</returns>
        /// <example>GET: BookingPage/Delete/5</example>
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        /// <summary>
        /// Confirms and executes the deletion of a booking and its related services (Admin only).
        /// </summary>
        /// <param name="id">The ID of the booking to delete</param>
        /// <returns>Redirects to the Index view after deletion</returns>
        /// <example>POST: BookingPage/Delete/5</example>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking != null)
            {
                _context.Booking_Services.RemoveRange(booking.BookingServices);
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Re-populate dropdowns if form is invalid
        /// </summary>
        private async Task RepopulateFormData(BookingFormViewModel model)
        {
            model.Owners = await _context.Owners.OrderBy(o => o.Name).ToListAsync();
            model.Pets = await _context.Pets.Include(p => p.Owner).OrderBy(p => p.Name).ToListAsync();
            model.Photographers = await _context.Photographers.Where(p => p.IsAvailable).OrderBy(p => p.Name).ToListAsync();
            model.Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        }

        /// <summary>
        /// Check if the current user is logged in
        /// </summary>
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));
        }

        /// <summary>
        /// Check if the current user is an admin
        /// </summary>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
