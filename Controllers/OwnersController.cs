using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.DTOs;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OwnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all owners including their pets.
        /// </summary>
        /// <returns>A list of owners with their pets.</returns>
        /// <example>GET: api/owners</example>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Owner>>> GetOwners()
        {
            return await _context.Owners
                .Include(o => o.Pets)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific owner by ID including their pets.
        /// </summary>
        /// <param name="id">The ID of the owner to retrieve.</param>
        /// <returns>The owner object with pets if found; 404 otherwise.</returns>
        /// <example>GET: api/owners/5</example>
        [HttpGet("{id}")]
        public async Task<ActionResult<Owner>> GetOwner(int id)
        {
            var owner = await _context.Owners
                .Include(o => o.Pets)
                .FirstOrDefaultAsync(o => o.OwnerId == id);

            if (owner == null)
                return NotFound();

            return owner;
        }

        /// <summary>
        /// Creates a new owner.
        /// </summary>
        /// <param name="owner">The owner object to create.</param>
        /// <returns>Created owner object along with a 201 status and location header.</returns>
        /// <example>POST: api/owners</example>
        [HttpPost]
        public async Task<ActionResult<Owner>> CreateOwner(Owner owner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOwner), new { id = owner.OwnerId }, owner);
        }

        /// <summary>
        /// Updates an existing owner.
        /// </summary>
        /// <param name="id">The ID of the owner to update.</param>
        /// <param name="owner">The updated owner object.</param>
        /// <returns>No content on success; 400 if IDs mismatch or model invalid; 404 if owner not found.</returns>
        /// <example>PUT: api/owners/5</example>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOwner(int id, Owner owner)
        {
            if (id != owner.OwnerId)
                return BadRequest("Owner ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(owner).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OwnerExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes an owner by ID.
        /// </summary>
        /// <param name="id">The ID of the owner to delete.</param>
        /// <returns>No content on success; 404 if owner not found.</returns>
        /// <example>DELETE: api/owners/5</example>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOwner(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound();

            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // <summary>
        /// Checks if an owner exists by ID.
        /// </summary>
        /// <param name="id">The owner ID to check.</param>
        /// <returns>True if owner exists, otherwise false.</returns>
        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.OwnerId == id);
        }
    }
}
