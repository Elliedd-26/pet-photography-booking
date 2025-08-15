using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// Controller for managing notifications sent to pet owners.
    /// </summary>
    [Route("NotificationsPage/[action]")]
    public class NotificationsPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays a list of all notifications.
        /// </summary>
        /// <returns>Notification list view</returns>
        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.RecipientOwner)
                .ToListAsync();
            return View(notifications);
        }

        /// <summary>
        /// Shows details for a specific notification.
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Notification details view</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications
                .Include(n => n.RecipientOwner)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null) return NotFound();

            return View(notification);
        }

        /// <summary>
        /// Renders the form to create a new notification.
        /// </summary>
        /// <returns>Create view</returns>
        public IActionResult Create()
        {
            ViewData["RecipientOwnerId"] = new SelectList(_context.Owners, "OwnerId", "Name");
            return View();
        }

        /// <summary>
        /// Handles submission of a new notification.
        /// </summary>
        /// <param name="notification">Notification model</param>
        /// <returns>Redirects to Index if successful, otherwise redisplays form</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationId,Title,Message,Type,IsRead,SentAt,RecipientOwnerId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipientOwnerId"] = new SelectList(_context.Owners, "OwnerId", "Name", notification.RecipientOwnerId);
            return View(notification);
        }

        /// <summary>
        /// Renders the form to edit an existing notification.
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Edit view</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            ViewData["RecipientOwnerId"] = new SelectList(_context.Owners, "OwnerId", "Name", notification.RecipientOwnerId);
            return View(notification);
        }

        /// <summary>
        /// Handles the submission of notification edits.
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <param name="notification">Updated notification data</param>
        /// <returns>Redirects to Index if successful, otherwise redisplays form</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationId,Title,Message,Type,IsRead,SentAt,RecipientOwnerId")] Notification notification)
        {
            if (id != notification.NotificationId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.NotificationId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["RecipientOwnerId"] = new SelectList(_context.Owners, "OwnerId", "Name", notification.RecipientOwnerId);
            return View(notification);
        }

        /// <summary>
        /// Displays a confirmation view to delete a notification.
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Delete confirmation view</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications
                .Include(n => n.RecipientOwner)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null) return NotFound();

            return View(notification);
        }

        /// <summary>
        /// Deletes the selected notification from the database.
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Redirects to Index view</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks whether a notification with the given ID exists.
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>True if exists, otherwise false</returns>
        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationId == id);
        }
    }
}
