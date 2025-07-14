using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// Controller to display database data for testing
    /// </summary>
    public class DataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all data in one page
        public async Task<IActionResult> Index()
        {
            ViewBag.Owners = await _context.Owners.ToListAsync();
            ViewBag.Pets = await _context.Pets.Include(p => p.Owner).ToListAsync();
            ViewBag.Notifications = await _context.Notifications.Include(n => n.Owner).ToListAsync();
            
            return View();
        }

        // Show owners only
        public async Task<IActionResult> Owners()
        {
            var owners = await _context.Owners
                .Include(o => o.Pets)
                .Include(o => o.Notifications)
                .ToListAsync();
            return View(owners);
        }

        // Show pets only
        public async Task<IActionResult> Pets()
        {
            var pets = await _context.Pets
                .Include(p => p.Owner)
                .ToListAsync();
            return View(pets);
        }

        // Show notifications only
        public async Task<IActionResult> Notifications()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Owner)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(notifications);
        }
    }
}