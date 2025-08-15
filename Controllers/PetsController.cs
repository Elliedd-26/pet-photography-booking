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

        /// <summary>
        /// Retrieves all pets with their owner's name.
        /// </summary>
        /// <returns>List of PetDTOs</returns>
        /// <example>
        /// GET: api/Pets
        /// </example>
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

        /// <summary>
        /// Retrieves a specific pet by its ID.
        /// </summary>
        /// <param name="id">Pet ID</param>
        /// <returns>PetDTO if found, 404 otherwise</returns>
        /// <example>
        /// GET: api/Pets/5
        /// </example>
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

        /// <summary>
        /// Retrieves pets belonging to a specific owner.
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <returns>List of Pet entities</returns>
        /// <example>
        /// GET: api/Pets/ByOwner/3
        /// </example>
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

        /// <summary>
        /// Retrieves pets by their species (e.g. Dog, Cat).
        /// </summary>
        /// <param name="species">Species name</param>
        /// <returns>List of PetDTOs</returns>
        /// <example>
        /// GET: api/Pets/BySpecies/Dog
        /// </example>
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

        /// <summary>
        /// Updates a pet with the specified ID.
        /// </summary>
        /// <param name="id">Pet ID</param>
        /// <param name="pet">Updated pet data</param>
        /// <returns>NoContent on success, error response otherwise</returns>
        /// <example>
        /// PUT: api/Pets/5
        /// </example>
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

        /// <summary>
        /// Creates a new pet.
        /// </summary>
        /// <param name="pet">Pet to add</param>
        /// <returns>Newly created pet</returns>
        /// <example>
        /// POST: api/Pets
        /// Body:
        /// {
        ///   "name": "Buddy",
        ///   "species": "Dog",
        ///   "breed": "Labrador",
        ///   "age": 3,
        ///   "ownerId": 1
        /// }
        /// </example>
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

        /// <summary>
        /// Deletes a pet by ID.
        /// </summary>
        /// <param name="id">Pet ID</param>
        /// <returns>NoContent if deleted, NotFound otherwise</returns>
        /// <example>
        /// DELETE: api/Pets/5
        /// </example>
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

        /// <summary>
        /// Checks if a pet exists by ID.
        /// </summary>
        /// <param name="id">Pet ID</param>
        /// <returns>True if exists, false otherwise</returns>
        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }
    }
}
