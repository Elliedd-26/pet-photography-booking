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
        /// Displays all photographers in the system.
        /// Accessible by all logged-in users.
        /// </summary>
        /// <returns>View containing a list of photographers</returns>
        /// <example>
        /// GET: PhotographersPage/Index
        /// </example>
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var photographers = await _context.Photographers.ToListAsync();
            return View(photographers);
        }

        /// <summary>
        /// Displays detailed information about a specific photographer, including bookings.
        /// Accessible by all logged-in users.
        /// </summary>
        /// <param name="id">Photographer ID</param>
        /// <returns>Details view of the selected photographer</returns>
        /// <example>
        /// GET: PhotographersPage/Details/5
        /// </example>
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
        /// Renders form to create a new photographer.
        /// Admin access only.
        /// </summary>
        /// <returns>View to create photographer</returns>
        /// <example>
        /// GET: PhotographersPage/Create
        /// </example>
        public IActionResult Create()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        /// <summary>
        /// Handles the creation of a new photographer.
        /// Admin access only.
        /// </summary>
        /// <param name="photographer">Photographer object</param>
        /// <returns>Redirects to Index if successful; otherwise returns the view with errors</returns>
        /// <example>
        /// POST: PhotographersPage/Create
        /// </example>
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
        /// Renders form to edit an existing photographer.
        /// Admin access only.
        /// </summary>
        /// <param name="id">Photographer ID</param>
        /// <returns>View to edit the photographer</returns>
        /// <example>
        /// GET: PhotographersPage/Edit/5
        /// </example>
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return Unauthorized();
            if (id == null) return NotFound();

            var photographer = await _context.Photographers.FindAsync(id);
            if (photographer == null) return NotFound();

            return View(photographer);
        }

        /// <summary>
        /// Handles the update of an existing photographer.
        /// Admin access only.
        /// </summary>
        /// <param name="id">Photographer ID</param>
        /// <param name="photographer">Photographer object with updated values</param>
        /// <returns>Redirects to Index if successful; otherwise returns the view with errors</returns>
        /// <example>
        /// POST: PhotographersPage/Edit/5
        /// </example>
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
        /// Displays confirmation view for deleting a photographer.
        /// Admin access only.
        /// </summary>
        /// <param name="id">Photographer ID</param>
        /// <returns>Delete confirmation view</returns>
        /// <example>
        /// GET: PhotographersPage/Delete/5
        /// </example>
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
        /// Confirms and executes deletion of a photographer.
        /// Admin access only.
        /// </summary>
        /// <param name="id">Photographer ID</param>
        /// <returns>Redirects to Index upon successful deletion</returns>
        /// <example>
        /// POST: PhotographersPage/Delete/5
        /// </example>

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
        /// Helper method to check if the user is logged in.
        /// </summary>
        /// <returns>True if logged in, otherwise false</returns>
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") != null;
        }

        /// <summary>
        /// Helper method to check if the user is an Admin.
        /// </summary>
        /// <returns>True if Admin, otherwise false</returns>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
