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

        // GET: PetPage
        // Returns a list of all pets with their owners.
        public async Task<IActionResult> Index()
        {
            var pets = await _context.Pets
                .Include(p => p.Owner)
                .ToListAsync();
            return View(pets);
        }

        // GET: PetPage/Details/5
        // Shows detailed info for a single pet.
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

        // GET: PetPage/Create
        // Displays the form to create a new pet.
        public async Task<IActionResult> Create()
        {
            // Get list of owners for dropdown
            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name");
            return View();
        }

        // POST: PetPage/Create
        // Handles form submission to create a new pet.
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
            
            // Repopulate dropdown in case of validation error
            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name", pet.OwnerId);
            return View(pet);
        }

        // GET: PetPage/Edit/5
        // Displays the form to edit an existing pet.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();

            // Get list of owners for dropdown
            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name", pet.OwnerId);
            return View(pet);
        }

        // POST: PetPage/Edit/5
        // Handles form submission to update an existing pet.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PetId,Name,Species,Breed,Age,Description,OwnerId")] Pet pet)
        {
            if (id != pet.PetId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(pet.PetId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate dropdown in case of validation error
            ViewData["OwnerId"] = new SelectList(await _context.Owners.ToListAsync(), "OwnerId", "Name", pet.OwnerId);
            return View(pet);
        }

        // GET: PetPage/Delete/5
        // Displays a confirmation view to delete a pet.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetId == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        // POST: PetPage/Delete/5
        // Handles the deletion after confirmation.
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

        // GET: PetPage/ByOwner/5
        // Shows all pets for a specific owner.
        public async Task<IActionResult> ByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest("Invalid owner ID.");

            var owner = await _context.Owners
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OwnerId == ownerId);

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

        // GET: PetPage/BySpecies?species=Dog
        // Shows all pets of a specific species.
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

        // Helper method to check if a pet exists in the database.
        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }
    }
}