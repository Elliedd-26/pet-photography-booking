using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// Controller to manage photographers (admin can create/edit/delete; all users can view).
    /// </summary>
    public class PhotographersPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhotographersPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display all photographers in the system.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var photographers = await _context.Photographers.ToListAsync();
            return View(photographers);
        }

        /// <summary>
        /// Show detailed info for a specific photographer.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (id == null) return NotFound();

            var photographer = await _context.Photographers
                .Include(p => p.Bookings)
                .FirstOrDefaultAsync(p => p.PhotographerId == id);

            if (photographer == null) return NotFound();
            return View(photographer);
        }

        /// <summary>
        /// Render create photographer form (Admin only).
        /// </summary>
        public IActionResult Create()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        /// <summary>
        /// Handle photographer creation (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Photographer photographer)
        {
            if (!IsAdmin()) return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Add(photographer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(photographer);
        }

        /// <summary>
        /// Render edit form for a photographer (Admin only).
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var photographer = await _context.Photographers.FindAsync(id);
            if (photographer == null) return NotFound();

            return View(photographer);
        }

        /// <summary>
        /// Handle photographer update (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Photographer photographer)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id != photographer.PhotographerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(photographer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Photographers.Any(p => p.PhotographerId == id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(photographer);
        }

        /// <summary>
        /// Show delete confirmation view (Admin only).
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var photographer = await _context.Photographers
                .FirstOrDefaultAsync(p => p.PhotographerId == id);

            if (photographer == null) return NotFound();

            return View(photographer);
        }

        /// <summary>
        /// Handle confirmed delete of a photographer (Admin only).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var photographer = await _context.Photographers.FindAsync(id);
            if (photographer != null)
            {
                _context.Photographers.Remove(photographer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Returns true if current user is logged in.
        /// </summary>
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") != null;
        }

        /// <summary>
        /// Returns true if current user is Admin.
        /// </summary>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
