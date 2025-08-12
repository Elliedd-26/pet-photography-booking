using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// MVC Controller for managing Owner data.
    /// Admins can create, edit, and delete; normal users can only view.
    /// </summary>
    [Route("OwnersPage")]
    public class OwnerPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays all pet owners in the system.
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var owners = await _context.Owners.ToListAsync();
            return View(owners);
        }

        /// <summary>
        /// Shows detailed view for a specific owner including pets, bookings, and notifications.
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (id == null) return NotFound();

            var owner = await _context.Owners
                .Include(o => o.Pets)
                .Include(o => o.Bookings)
                .Include(o => o.Notifications)
                .FirstOrDefaultAsync(m => m.OwnerId == id);

            if (owner == null) return NotFound();
            return View(owner);
        }

        /// <summary>
        /// Renders form to create a new owner (Admin only).
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        /// <summary>
        /// Handles creation of a new owner (Admin only).
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnerId,Name,Email,PhoneNumber,Address")] Owner owner)
        {
            if (!IsAdmin()) return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Add(owner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(owner);
        }

        /// <summary>
        /// Renders form to edit an existing owner (Admin only).
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var owner = await _context.Owners.FindAsync(id);
            if (owner == null) return NotFound();

            return View(owner);
        }

        /// <summary>
        /// Handles update of an owner (Admin only).
        /// </summary>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OwnerId,Name,Email,PhoneNumber,Address")] Owner owner)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id != owner.OwnerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(owner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnerExists(owner.OwnerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(owner);
        }

        /// <summary>
        /// Renders confirmation page for deleting an owner (Admin only).
        /// </summary>
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var owner = await _context.Owners.FirstOrDefaultAsync(m => m.OwnerId == id);
            if (owner == null) return NotFound();

            return View(owner);
        }

        /// <summary>
        /// Handles deletion of an owner from the system (Admin only).
        /// </summary>
        [HttpPost("Delete/{id}"), ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var owner = await _context.Owners.FindAsync(id);
            if (owner != null)
            {
                _context.Owners.Remove(owner);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if an owner exists.
        /// </summary>
        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.OwnerId == id);
        }

        /// <summary>
        /// Returns true if user is logged in.
        /// </summary>
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") != null;
        }

        /// <summary>
        /// Returns true if user has Admin role.
        /// </summary>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
