using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers
{
    public class PetPageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PetPageController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: PetPage/Index
        // Visible to all logged-in users
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var pets = await _context.Pets.Include(p => p.Owner).ToListAsync();
            return View(pets);
        }

        // GET: PetPage/Details/5
        // Visible to all logged-in users
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            if (id == null) return NotFound();

            var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        // GET: PetPage/Create
        // Allow all logged-in users to access creation form
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            ViewBag.Owners = _context.Owners.ToList();
            return View();
        }

        // POST: PetPage/Create
        // Allow all logged-in users to submit pet creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet, IFormFile? photo)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            // Handle photo upload if file is provided
            if (photo != null && photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                pet.PhotoPath = "/uploads/" + fileName;
            }

            _context.Add(pet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: PetPage/Edit/5
        // Only Admins can access pet edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (!IsAdmin()) return Unauthorized();

            if (id == null) return NotFound();

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();

            ViewBag.Owners = _context.Owners.ToList();
            return View(pet);
        }

        // POST: PetPage/Edit/5
        // Only Admins can update pet info
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet, IFormFile? photo)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (!IsAdmin()) return Unauthorized();

            if (id != pet.PetId) return NotFound();

            var existingPet = await _context.Pets.FindAsync(id);
            if (existingPet == null) return NotFound();

            // Update fields
            existingPet.Name = pet.Name;
            existingPet.Species = pet.Species;
            existingPet.Breed = pet.Breed;
            existingPet.Age = pet.Age;
            existingPet.Description = pet.Description;
            existingPet.OwnerId = pet.OwnerId;

            // Handle new photo upload
            if (photo != null && photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                existingPet.PhotoPath = "/uploads/" + fileName;
            }

            _context.Update(existingPet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: PetPage/Delete/5
        // Only Admins can view delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (!IsAdmin()) return Unauthorized();

            if (id == null) return NotFound();

            var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        // POST: PetPage/Delete/5
        // Only Admins can delete a pet
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (!IsAdmin()) return Unauthorized();

            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if current user is logged in
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));
        }

        // Helper method to check if current user is an Admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
