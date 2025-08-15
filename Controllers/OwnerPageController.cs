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
        /// <returns>A view listing all owners.</returns>
        /// <example>GET: OwnersPage</example>
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
        /// <param name="id">The ID of the owner to display.</param>
        /// <returns>A view displaying details of the owner.</returns>
        /// <example>GET: OwnersPage/Details/5</example>
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
        /// <returns>The create owner view.</returns>
        /// <example>GET: OwnersPage/Create</example>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        /// <summary>
        /// Handles creation of a new owner (Admin only).
        /// </summary>
        /// <param name="owner">The Owner object to create.</param>
        /// <returns>Redirects to Index on success, or redisplays the form on failure.</returns>
        /// <example>POST: OwnersPage/Create</example>
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
        /// <param name="id">The ID of the owner to edit.</param>
        /// <returns>The edit owner view.</returns>
        /// <example>GET: OwnersPage/Edit/5</example>
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
        /// <param name="id">The ID of the owner being updated.</param>
        /// <param name="owner">The updated Owner object.</param>
        /// <returns>Redirects to Index on success, or redisplays the form on failure.</returns>
        /// <example>POST: OwnersPage/Edit/5</example>
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
        /// <param name="id">The ID of the owner to delete.</param>
        /// <returns>The delete confirmation view.</returns>
        /// <example>GET: OwnersPage/Delete/5</example>
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
        /// <param name="id">The ID of the owner to delete.</param>
        /// <returns>Redirects to Index after deletion.</returns>
        /// <example>POST: OwnersPage/Delete/5</example>
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
        /// <param name="id">The ID of the owner to check.</param>
        /// <returns>True if owner exists, otherwise false.</returns>
        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.OwnerId == id);
        }

        /// <summary>
        /// Returns true if user is logged in.
        /// </summary>
        /// <returns>True if logged in; otherwise false.</returns>
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") != null;
        }

        /// <summary>
        /// Returns true if user has Admin role.
        /// </summary>
        /// <returns>True if user role is Admin; otherwise false.</returns>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
