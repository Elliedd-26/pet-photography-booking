using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetPhotographyApp.Controllers
{
    [Route("NotificationsPage/[action]")]
    public class NotificationsPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.RecipientOwner)
                .ToListAsync();
            return View(notifications);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications
                .Include(n => n.RecipientOwner)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null) return NotFound();

            return View(notification);
        }

        public IActionResult Create()
        {
            ViewData["RecipientOwnerId"] = new SelectList(_context.Owners, "OwnerId", "Name");
            return View();
        }

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            ViewData["RecipientOwnerId"] = new SelectList(_context.Owners, "OwnerId", "Name", notification.RecipientOwnerId);
            return View(notification);
        }

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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications
                .Include(n => n.RecipientOwner)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null) return NotFound();

            return View(notification);
        }

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

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationId == id);
        }
    }
}
