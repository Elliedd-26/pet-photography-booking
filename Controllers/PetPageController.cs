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

        /// <summary>
        /// Displays a list of all pets with their owners.
        /// </summary>
        /// <returns>View with list of pets.</returns>
        /// <example>GET: PetPage/Index</example>
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            var pets = await _context.Pets.Include(p => p.Owner).ToListAsync();
            return View(pets);
        }

        /// <summary>
        /// Shows details for a specific pet.
        /// </summary>
        /// <param name="id">The ID of the pet.</param>
        /// <returns>View with pet details or NotFound if pet doesn't exist.</returns>
        /// <example>GET: PetPage/Details/5</example>
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            if (id == null) return NotFound();

            var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        /// <summary>
        /// Displays form for creating a new pet.
        /// </summary>
        /// <returns>View with creation form.</returns>
        /// <example>GET: PetPage/Create</example>
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");

            ViewBag.Owners = _context.Owners.ToList();
            return View();
        }

        /// <summary>
        /// Processes new pet creation.
        /// </summary>
        /// <param name="pet">The pet model data.</param>
        /// <param name="photo">Optional uploaded photo.</param>
        /// <returns>Redirects to index on success or reloads form on failure.</returns>
        /// <example>POST: PetPage/Create</example>
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

        /// <summary>
        /// Displays form to edit an existing pet.
        /// </summary>
        /// <param name="id">The ID of the pet to edit.</param>
        /// <returns>View with edit form or NotFound/Unauthorized if not accessible.</returns>
        /// <example>GET: PetPage/Edit/5</example>
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

        /// <summary>
        /// Processes updates to an existing pet.
        /// </summary>
        /// <param name="id">The ID of the pet being updated.</param>
        /// <param name="pet">Updated pet data.</param>
        /// <param name="photo">Optional new photo file.</param>
        /// <returns>Redirects to index on success, or NotFound/Unauthorized on failure.</returns>
        /// <example>POST: PetPage/Edit/5</example>
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

        /// <summary>
        /// Shows confirmation page for deleting a pet.
        /// </summary>
        /// <param name="id">The ID of the pet to delete.</param>
        /// <returns>View with pet details or NotFound/Unauthorized if not accessible.</returns>
        /// <example>GET: PetPage/Delete/5</example>
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            if (!IsAdmin()) return Unauthorized();

            if (id == null) return NotFound();

            var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        /// <summary>
        /// Deletes the specified pet.
        /// </summary>
        /// <param name="id">The ID of the pet to delete.</param>
        /// <returns>Redirects to index after deletion or NotFound/Unauthorized if not accessible.</returns>
        /// <example>POST: PetPage/Delete/5</example>
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

        /// <summary>
        /// Checks if the user is logged in.
        /// </summary>
        /// <returns>True if logged in; otherwise false.</returns>
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));
        }

        /// <summary>
        /// Checks if the user has Admin role.
        /// </summary>
        /// <returns>True if user is Admin; otherwise false.</returns>
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}
