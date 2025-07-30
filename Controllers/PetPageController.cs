using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.Models.ViewModels;

namespace PetPhotographyApp.Controllers
{
    public class PetPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PetPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pets = await _context.Pets
                .Include(p => p.Owner)
                .ToListAsync();
            return View(pets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Photographer)
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null) return NotFound();

            return View(pet);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PetId,Name,Species,Breed,Age,Description,OwnerId")] Pet pet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name", pet.OwnerId);
            return View(pet);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name", pet.OwnerId);
            return View(pet);
        }

        // ✅ POST: PetPage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PetId,Name,Species,Breed,Age,Description,OwnerId")] Pet pet)
        {
            Console.WriteLine("🎯 Entered Edit POST method");

            if (id != pet.PetId)
            {
                Console.WriteLine("❌ ID mismatch");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState Invalid");
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($" - {kvp.Key}: {error.ErrorMessage}");
                    }
                }

                ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name", pet.OwnerId);
                return View(pet);
            }

            try
            {
                _context.Update(pet);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Pet updated successfully");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetExists(pet.PetId))
                {
                    Console.WriteLine("❌ Pet not found during update");
                    return NotFound();
                }
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest("Invalid owner ID.");

            var owner = await _context.Owners.AsNoTracking().FirstOrDefaultAsync(o => o.OwnerId == ownerId);
            if (owner == null)
                return NotFound($"Owner with ID {ownerId} not found.");

            var pets = await _context.Pets
                .Where(p => p.OwnerId == ownerId)
                .Include(p => p.Owner)
                .ToListAsync();

            ViewData["OwnerName"] = owner.Name;
            ViewData["OwnerId"] = ownerId;

            return View(pets);
        }

        public async Task<IActionResult> BySpecies(string species)
        {
            if (string.IsNullOrEmpty(species))
                return BadRequest("Species parameter is required.");

            var pets = await _context.Pets
                .Where(p => p.Species.ToLower() == species.ToLower())
                .Include(p => p.Owner)
                .ToListAsync();

            if (!pets.Any())
                return NotFound($"No pets found of species '{species}'.");

            ViewData["Species"] = species;

            return View(pets);
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }
    }
}
