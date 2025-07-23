using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.Models.ViewModels;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// MVC Controller for managing booking-related web pages
    /// Handles user interface operations for viewing, creating, editing, and deleting bookings
    /// Provides server-side rendering of booking management pages
    /// </summary>
    public class BookingPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor - Initializes controller with database context dependency
        /// </summary>
        /// <param name="context">Entity Framework database context for data operations</param>
        public BookingPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: BookingPage/Index
        /// Displays the main booking list page with all bookings in the system
        /// Shows summary information for each booking in a table format
        /// </summary>
        /// <returns>View with list of booking summary view models</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Fetch all bookings with related entity data for display
            var bookingViewModels = await _context.Bookings
                .Include(b => b.Owner)           // Fixed: Changed from Owner to Owner
                .Include(b => b.Pet)             // Added: Include pet information
                .Include(b => b.Photographer)
                .Select(b => new BookingSummaryViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    Location = b.Location ?? string.Empty,
                    OwnerName = b.Owner != null ? b.Owner.Name : "Unknown Owner",   // Fixed: Owner to Owner
                    PetName = b.Pet != null ? b.Pet.Name : "Unknown Pet",           // Added: Pet name
                    PhotographerName = b.Photographer != null ? b.Photographer.Name : "Unknown Photographer",
                    ServiceCount = _context.Booking_Services.Count(bs => bs.BookingId == b.BookingId)
                })
                .OrderByDescending(b => b.BookingDate) // Most recent bookings first
                .ToListAsync();

            return View(bookingViewModels);
        }

        /// <summary>
        /// GET: BookingPage/Details/5
        /// Displays detailed information for a specific booking
        /// Shows all booking details, associated services, and related entities
        /// </summary>
        /// <param name="id">Unique identifier of the booking to display</param>
        /// <returns>View with detailed booking information or NotFound if booking doesn't exist</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch booking with all related data for detailed view
            var booking = await _context.Bookings
                .Include(b => b.Owner)           // Fixed: Changed from Owner to Owner
                .Include(b => b.Pet)             // Added: Include pet details
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound($"Booking with ID {id} not found.");

            // Create detailed view model with all booking information
            var viewModel = new BookingDetailsViewModel
            {
                Booking = new DTOs.BookingDTO
                {
                    BookingId = booking.BookingId,
                    BookingDate = booking.BookingDate,
                    Location = booking.Location,
                    OwnerId = booking.OwnerId,                      // Fixed: Changed from OwnerId
                    OwnerName = booking.Owner.Name,                 // Fixed: Changed from OwnerName
                    PetId = booking.PetId,                          // Added: Pet ID
                    PetName = booking.Pet.Name,                     // Added: Pet name
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
        /// GET: BookingPage/Create
        /// Displays the booking creation form with available owners, pets, photographers, and services
        /// Populates dropdown lists for user selection
        /// </summary>
        /// <returns>View with empty booking form and populated dropdown options</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Prepare view model with all necessary data for booking creation
            var viewModel = new BookingFormViewModel
            {
                Owners = await _context.Owners                      // Fixed: Changed from Owners to Owners
                    .OrderBy(o => o.Name)
                    .ToListAsync(),
                Pets = await _context.Pets                          // Added: Include pets for selection
                    .Include(p => p.Owner)
                    .OrderBy(p => p.Name)
                    .ToListAsync(),
                Photographers = await _context.Photographers
                    .Where(p => p.IsAvailable)                      // Only show available photographers
                    .OrderBy(p => p.Name)
                    .ToListAsync(),
                Services = await _context.Services
                    .Where(s => s.IsActive)                         // Only show active services
                    .OrderBy(s => s.Name)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: BookingPage/Create
        /// Processes booking creation form submission
        /// Validates input data and creates new booking with associated services
        /// </summary>
        /// <param name="model">Booking form data submitted by user</param>
        /// <returns>Redirect to Index on success, or redisplay form with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdown data if validation fails
                await RepopulateFormData(model);
                return View(model);
            }

            // Validate that selected entities exist
            var owner = await _context.Owners.FindAsync(model.OwnerId);         // Fixed: Changed from OwnerId
            var pet = await _context.Pets.FindAsync(model.PetId);               // Added: Pet validation
            var photographer = await _context.Photographers.FindAsync(model.PhotographerId);

            if (owner == null)
            {
                ModelState.AddModelError(nameof(model.OwnerId), "Selected owner not found.");
                await RepopulateFormData(model);
                return View(model);
            }

            if (pet == null)
            {
                ModelState.AddModelError(nameof(model.PetId), "Selected pet not found.");
                await RepopulateFormData(model);
                return View(model);
            }

            if (photographer == null)
            {
                ModelState.AddModelError(nameof(model.PhotographerId), "Selected photographer not found.");
                await RepopulateFormData(model);
                return View(model);
            }

            // Verify pet belongs to selected owner
            if (pet.OwnerId != model.OwnerId)
            {
                ModelState.AddModelError(nameof(model.PetId), "Selected pet does not belong to the selected owner.");
                await RepopulateFormData(model);
                return View(model);
            }

            // Create new booking entity
            var booking = new Booking
            {
                BookingDate = model.BookingDate,
                Location = model.Location,
                OwnerId = model.OwnerId,                            // Fixed: Changed from OwnerId
                PetId = model.PetId,                                // Added: Pet ID assignment
                PhotographerId = model.PhotographerId,
                Status = "Pending",                                 // Set initial status
                Notes = model.Notes                                 // Added: Notes field
            };

            // Add booking to database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Add selected services to the booking
            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    _context.Booking_Services.Add(new Booking_Service
                    {
                        BookingId = booking.BookingId,
                        ServiceId = serviceId,
                        Status = "Pending"                          // Set initial service status
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Set success message and redirect
            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: BookingPage/Edit/5
        /// Displays the booking editing form pre-populated with existing booking data
        /// Allows modification of booking details and service selections
        /// </summary>
        /// <param name="id">ID of the booking to edit</param>
        /// <returns>View with populated booking form or NotFound if booking doesn't exist</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch existing booking with related services
            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound($"Booking with ID {id} not found.");

            // Create form model with existing data
            var model = new BookingFormViewModel
            {
                BookingId = booking.BookingId,
                BookingDate = booking.BookingDate,
                Location = booking.Location,
                OwnerId = booking.OwnerId,                          // Fixed: Changed from OwnerId
                PetId = booking.PetId,                              // Added: Pet ID
                PhotographerId = booking.PhotographerId,
                Notes = booking.Notes,                              // Added: Notes field
                SelectedServiceIds = booking.BookingServices.Select(bs => bs.ServiceId).ToList()
            };

            // Populate dropdown data
            await RepopulateFormData(model);
            return View(model);
        }

        /// <summary>
        /// POST: BookingPage/Edit/5
        /// Processes booking update form submission
        /// Updates existing booking with new information and service selections
        /// </summary>
        /// <param name="id">ID of the booking to update</param>
        /// <param name="model">Updated booking form data</param>
        /// <returns>Redirect to Index on success, or redisplay form with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingFormViewModel model)
        {
            if (id != model.BookingId)
                return BadRequest("Booking ID mismatch.");

            if (!ModelState.IsValid)
            {
                await RepopulateFormData(model);
                return View(model);
            }

            // Fetch existing booking with services
            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound($"Booking with ID {id} not found.");

            // Validate related entities exist
            var owner = await _context.Owners.FindAsync(model.OwnerId);         // Fixed: Changed from OwnerId
            var pet = await _context.Pets.FindAsync(model.PetId);               // Added: Pet validation
            var photographer = await _context.Photographers.FindAsync(model.PhotographerId);

            if (owner == null || pet == null || photographer == null)
            {
                ModelState.AddModelError("", "One or more selected entities not found.");
                await RepopulateFormData(model);
                return View(model);
            }

            // Verify pet belongs to selected owner
            if (pet.OwnerId != model.OwnerId)
            {
                ModelState.AddModelError(nameof(model.PetId), "Selected pet does not belong to the selected owner.");
                await RepopulateFormData(model);
                return View(model);
            }

            // Update booking properties
            booking.BookingDate = model.BookingDate;
            booking.Location = model.Location;
            booking.OwnerId = model.OwnerId;                        // Fixed: Changed from OwnerId
            booking.PetId = model.PetId;                            // Added: Pet ID update
            booking.PhotographerId = model.PhotographerId;
            booking.Notes = model.Notes;                            // Added: Notes update

            // Remove existing service associations
            _context.Booking_Services.RemoveRange(booking.BookingServices);

            // Add updated service selections
            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                booking.BookingServices = model.SelectedServiceIds.Select(sid => new Booking_Service
                {
                    BookingId = booking.BookingId,
                    ServiceId = sid,
                    Status = "Pending"
                }).ToList();
            }

            // Save changes
            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: BookingPage/Delete/5
        /// Displays booking deletion confirmation page
        /// Shows booking details before permanent removal
        /// </summary>
        /// <param name="id">ID of the booking to delete</param>
        /// <returns>View with booking confirmation details or NotFound if booking doesn't exist</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Owner)                              // Fixed: Changed from Owner to Owner
                .Include(b => b.Pet)                                // Added: Include pet
                .Include(b => b.Photographer)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound($"Booking with ID {id} not found.");

            return View(booking);
        }

        /// <summary>
        /// POST: BookingPage/Delete/5
        /// Processes booking deletion confirmation
        /// Removes booking and all associated service relationships
        /// </summary>
        /// <param name="id">ID of the booking to delete</param>
        /// <returns>Redirect to Index after successful deletion</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound($"Booking with ID {id} not found.");

            // Remove associated booking services first (due to foreign key constraints)
            _context.Booking_Services.RemoveRange(booking.BookingServices);

            // Remove the booking itself
            _context.Bookings.Remove(booking);

            // Save all changes
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: BookingPage/ByService/5
        /// Displays all bookings that include a specific service
        /// Useful for analyzing service usage and managing service-specific bookings
        /// </summary>
        /// <param name="serviceId">ID of the service to filter by</param>
        /// <returns>View with bookings filtered by service or error if service not found</returns>
        [HttpGet]
        public async Task<IActionResult> ByService(int serviceId)
        {
            if (serviceId <= 0)
                return BadRequest("Invalid service ID.");

            // Verify service exists
            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
                return NotFound($"Service with ID {serviceId} not found.");

            // Get all bookings that include this service
            var bookings = await _context.Bookings
                .Where(b => b.BookingServices.Any(bs => bs.ServiceId == serviceId))
                .Include(b => b.Owner)                              // Fixed: Changed from Owner to Owner
                .Include(b => b.Pet)                                // Added: Include pet
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            if (!bookings.Any())
                return NotFound($"No bookings found for service '{service.Name}'.");

            // Convert to view models
            var bookingViewModels = bookings.Select(b => new BookingSummaryViewModel
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",       // Fixed: Owner to Owner
                PetName = b.Pet?.Name ?? "Unknown Pet",             // Added: Pet name
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
                ServiceCount = b.BookingServices?.Count ?? 0
            }).ToList();

            ViewData["ServiceId"] = serviceId;
            ViewData["ServiceName"] = service.Name;

            return View(bookingViewModels);
        }

        /// <summary>
        /// GET: BookingPage/ByPhotographer/5
        /// Displays all bookings assigned to a specific photographer
        /// Useful for photographer workload management and scheduling
        /// </summary>
        /// <param name="photographerId">ID of the photographer to filter by</param>
        /// <returns>View with photographer's bookings or error if photographer not found</returns>
        [HttpGet]
        public async Task<IActionResult> ByPhotographer(int photographerId)
        {
            if (photographerId <= 0)
                return BadRequest("Invalid photographer ID.");

            // Verify photographer exists
            var photographer = await _context.Photographers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PhotographerId == photographerId);

            if (photographer == null)
                return NotFound($"Photographer with ID {photographerId} not found.");

            // Get all bookings for this photographer
            var bookings = await _context.Bookings
                .Where(b => b.PhotographerId == photographerId)
                .Include(b => b.Owner)                              // Fixed: Changed from Owner to Owner
                .Include(b => b.Pet)                                // Added: Include pet
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            if (!bookings.Any())
                return NotFound($"No bookings found for photographer '{photographer.Name}'.");

            // Convert to view models
            var bookingViewModels = bookings.Select(b => new BookingSummaryViewModel
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",       // Fixed: Owner to Owner
                PetName = b.Pet?.Name ?? "Unknown Pet",             // Added: Pet name
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
                ServiceCount = b.BookingServices?.Count ?? 0
            }).ToList();

            ViewData["PhotographerName"] = photographer.Name;
            ViewData["PhotographerId"] = photographerId;

            return View(bookingViewModels);
        }

        /// <summary>
        /// GET: BookingPage/ByOwner/5
        /// Displays all bookings made by a specific pet owner
        /// Allows owners to view their booking history
        /// </summary>
        /// <param name="ownerId">ID of the owner to filter by</param>
        /// <returns>View with owner's bookings or error if owner not found</returns>
        [HttpGet]
        public async Task<IActionResult> ByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest("Invalid owner ID.");

            // Verify owner exists
            var owner = await _context.Owners
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OwnerId == ownerId);

            if (owner == null)
                return NotFound($"Owner with ID {ownerId} not found.");

            // Get all bookings for this owner
            var bookings = await _context.Bookings
                .Where(b => b.OwnerId == ownerId)
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            if (!bookings.Any())
                return NotFound($"No bookings found for owner '{owner.Name}'.");

            // Convert to view models
            var bookingViewModels = bookings.Select(b => new BookingSummaryViewModel
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",
                PetName = b.Pet?.Name ?? "Unknown Pet",
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
                ServiceCount = b.BookingServices?.Count ?? 0
            }).ToList();

            ViewData["OwnerName"] = owner.Name;
            ViewData["OwnerId"] = ownerId;

            return View(bookingViewModels);
        }

        /// <summary>
        /// Private helper method to repopulate form dropdown data
        /// Used when form validation fails and form needs to be redisplayed
        /// </summary>
        /// <param name="model">Form model to populate</param>
        /// <returns>Task for async operation</returns>
        private async Task RepopulateFormData(BookingFormViewModel model)
        {
            model.Owners = await _context.Owners                   // Fixed: Changed from Owners
                .OrderBy(o => o.Name)
                .ToListAsync();

            model.Pets = await _context.Pets                       // Added: Pet population
                .Include(p => p.Owner)
                .OrderBy(p => p.Name)
                .ToListAsync();

            model.Photographers = await _context.Photographers
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Name)
                .ToListAsync();

            model.Services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}