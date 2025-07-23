using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        /// Returns a list of pets in the system.
        /// </summary>
        /// <example>
        /// GET http://localhost:7198/api/Pets -> [{"PetId":1,"Name":"Fluffy","Species":"Cat","OwnerName":"Lisa Smith"}, ...]
        /// </example>
        /// <returns>
        /// A list of Pet objects with owner information.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetDTO>>> GetPets()
        {
            var pets = await _context.Pets
                .Include(p => p.Owner)
                .ToListAsync();

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
        /// Returns a single pet by ID.
        /// </summary>
        /// <example>
        /// GET http://localhost:7198/api/Pets/5 -> {"PetId":5,"Name":"Fluffy","Species":"Cat","OwnerName":"Lisa Smith"}
        /// </example>
        /// <returns>
        /// A Pet object if found; otherwise, 404 Not Found.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Pet>> GetPet(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
            {
                return NotFound();
            }

            return pet;
        }

        /// <summary>
        /// Returns all pets for a specific owner.
        /// </summary>
        /// <example>
        /// GET http://localhost:7198/api/Pets/ByOwner/3 -> [{"PetId":1,"Name":"Fluffy","Species":"Cat"}, ...]
        /// </example>
        /// <param name="ownerId">The ID of the owner.</param>
        /// <returns>
        /// A list of Pet objects for the specified owner.
        /// </returns>
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
        /// Returns pets by species (e.g., "Dog", "Cat").
        /// </summary>
        /// <example>
        /// GET http://localhost:7198/api/Pets/BySpecies/Dog -> [{"PetId":1,"Name":"Buddy","Species":"Dog"}, ...]
        /// </example>
        /// <param name="species">The species to search for.</param>
        /// <returns>
        /// A list of Pet objects of the specified species.
        /// </returns>
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
        /// Updates an existing pet in the system.
        /// </summary>
        /// <example>
        /// PUT http://localhost:7198/api/Pets/5
        /// Body: {"PetId":5,"Name":"Updated Name","Species":"Dog"}
        /// </example>
        /// <returns>
        /// NoContent on success, BadRequest if ID mismatch, NotFound if pet does not exist.
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPet(int id, Pet pet)
        {
            if (id != pet.PetId)
            {
                return BadRequest();
            }

            // Verify owner exists
            var ownerExists = await _context.Owners.AnyAsync(o => o.OwnerId == pet.OwnerId);
            if (!ownerExists)
            {
                return BadRequest("Invalid OwnerId: Owner does not exist.");
            }

            _context.Entry(pet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Adds a new pet to the system.
        /// </summary>
        /// <example>
        /// POST http://localhost:7198/api/Pets
        /// Body: {"Name":"Buddy","Species":"Dog","Breed":"Golden Retriever","Age":3,"OwnerId":1}
        /// </example>
        /// <returns>
        /// The newly created Pet object with location header.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<Pet>> PostPet(Pet pet)
        {
            // Verify owner exists
            var ownerExists = await _context.Owners.AnyAsync(o => o.OwnerId == pet.OwnerId);
            if (!ownerExists)
            {
                return BadRequest("Invalid OwnerId: Owner does not exist.");
            }

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPet", new { id = pet.PetId }, pet);
        }

        /// <summary>
        /// Deletes a pet by ID.
        /// </summary>
        /// <example>
        /// DELETE http://localhost:7198/api/Pets/5
        /// </example>
        /// <returns>
        /// NoContent on success, NotFound if pet does not exist.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

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