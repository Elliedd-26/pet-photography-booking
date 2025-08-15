using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.DTOs;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// Controller to manage service-related operations (admin can modify, users can view).
    /// </summary>
    public class ServicesPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays all available services.
        /// </summary>
        /// <returns>View containing a list of services.</returns>
        /// <example>
        /// GET: /ServicesPage/Index
        /// </example>
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        /// <summary>
        /// Shows details for a specific service.
        /// </summary>
        /// <param name="id">The ID of the service to view.</param>
        /// <returns>View showing service details.</returns>
        /// <example>
        /// GET: /ServicesPage/Details/3
        /// </example>
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null) return NotFound();

            var dto = new ServiceDTO
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price
            };

            return View(dto);
        }

        /// <summary>
        /// Renders the service creation form (Admin only).
        /// </summary>
        /// <returns>View for creating a new service.</returns>
        /// <example>
        /// GET: /ServicesPage/Create
        /// </example>
        public IActionResult Create()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        /// <summary>
        /// Handles service creation (Admin only).
        /// </summary>
        /// <param name="serviceDto">The service data to be created.</param>
        /// <returns>Redirects to Index on success or reloads the form on failure.</returns>
        /// <example>
        /// POST: /ServicesPage/Create
        /// </example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price")] ServiceDTO serviceDto)
        {
            if (!IsAdmin()) return Unauthorized();

            if (ModelState.IsValid)
            {
                var service = new Service
                {
                    Name = serviceDto.Name,
                    Price = serviceDto.Price
                };
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serviceDto);
        }

        /// <summary>
        /// Renders the service edit form (Admin only).
        /// </summary>
        /// <param name="id">The ID of the service to edit.</param>
        /// <returns>View for editing the service.</returns>
        /// <example>
        /// GET: /ServicesPage/Edit/5
        /// </example>
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            var dto = new ServiceDTO
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price
            };

            return View(dto);
        }

        /// <summary>
        /// Handles service update (Admin only).
        /// </summary>
        /// <param name="id">The ID of the service being updated.</param>
        /// <param name="serviceDto">The updated service data.</param>
        /// <returns>Redirects to Index on success or reloads the form on failure.</returns>
        /// <example>
        /// POST: /ServicesPage/Edit/5
        /// </example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,Name,Price")] ServiceDTO serviceDto)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id != serviceDto.ServiceId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var service = await _context.Services.FindAsync(id);
                    if (service == null) return NotFound();

                    service.Name = serviceDto.Name;
                    service.Price = serviceDto.Price;

                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Services.Any(e => e.ServiceId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(serviceDto);
        }

        /// <summary>
        /// Renders delete confirmation view (Admin only).
        /// </summary>
        /// <param name="id">The ID of the service to delete.</param>
        /// <returns>View showing delete confirmation.</returns>
        /// <example>
        /// GET: /ServicesPage/Delete/5
        /// </example>
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null) return NotFound();

            var dto = new ServiceDTO
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price
            };

            return View(dto);
        }

        /// <summary>
        /// Executes service deletion after confirmation (Admin only).
        /// </summary>
        /// <param name="id">The ID of the service to delete.</param>
        /// <returns>Redirects to Index on success.</returns>
        /// <example>
        /// POST: /ServicesPage/Delete/5
        /// </example>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if a user is logged in.
        /// </summary>
        /// <returns>True if user is logged in; otherwise, false.</returns>
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") != null;
        }

        /// <summary>
        /// Checks if the logged-in user is an admin.
        /// </summary>
        /// <returns>True if user is an admin; otherwise, false.</returns>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
