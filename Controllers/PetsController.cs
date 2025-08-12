using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.DTOs;

namespace PetPhotographyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetDTO>>> GetPets()
        {
            var pets = await _context.Pets.Include(p => p.Owner).ToListAsync();

            var petDtos = pets.Select(p => new PetDTO
            {
                PetId = p.PetId,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                Age = p.Age,
                OwnerName = p.Owner.Name
            }).ToList();

            return Ok(petDtos);
        }

        // GET: api/Pets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PetDTO>> GetPet(int id)
        {
            var pet = await _context.Pets.Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
                return NotFound();

            var petDto = new PetDTO
            {
                PetId = pet.PetId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Age = pet.Age,
                OwnerName = pet.Owner?.Name ?? "Unknown"
            };

            return Ok(petDto);
        }

        // GET: api/Pets/ByOwner/3
        [HttpGet("ByOwner/{ownerId}")]
        public async Task<ActionResult<IEnumerable<Pet>>> GetPetsByOwner(int ownerId)
        {
            var pets = await _context.Pets
                .Where(p => p.OwnerId == ownerId)
                .Include(p => p.Owner)
                .ToListAsync();

            if (!pets.Any())
                return NotFound($"No pets found for owner ID {ownerId}.");

            return Ok(pets);
        }

        // GET: api/Pets/BySpecies/Dog
        [HttpGet("BySpecies/{species}")]
        public async Task<ActionResult<IEnumerable<PetDTO>>> GetPetsBySpecies(string species)
        {
            var pets = await _context.Pets
                .Where(p => p.Species.ToLower() == species.ToLower())
                .Include(p => p.Owner)
                .ToListAsync();

            if (!pets.Any())
                return NotFound($"No pets found of species '{species}'.");

            var petDtos = pets.Select(p => new PetDTO
            {
                PetId = p.PetId,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                Age = p.Age,
                OwnerName = p.Owner.Name
            }).ToList();

            return Ok(petDtos);
        }

        // PUT: api/Pets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPet(int id, Pet pet)
        {
            if (id != pet.PetId)
                return BadRequest();

            var ownerExists = await _context.Owners.AnyAsync(o => o.OwnerId == pet.OwnerId);
            if (!ownerExists)
                return BadRequest("Invalid OwnerId: Owner does not exist.");

            _context.Entry(pet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Pets
        [HttpPost]
        public async Task<ActionResult<Pet>> PostPet(Pet pet)
        {
            var ownerExists = await _context.Owners.AnyAsync(o => o.OwnerId == pet.OwnerId);
            if (!ownerExists)
                return BadRequest("Invalid OwnerId: Owner does not exist.");

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPet", new { id = pet.PetId }, pet);
        }

        // DELETE: api/Pets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
                return NotFound();

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }
    }
}
