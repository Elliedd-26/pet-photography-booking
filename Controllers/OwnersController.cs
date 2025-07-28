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

        // GET: api/owners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Owner>>> GetOwners()
        {
            return await _context.Owners
                .Include(o => o.Pets)
                .ToListAsync();
        }

        // GET: api/owners/5
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

        // POST: api/owners
        [HttpPost]
        public async Task<ActionResult<Owner>> CreateOwner(Owner owner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOwner), new { id = owner.OwnerId }, owner);
        }

        // PUT: api/owners/5
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

        // DELETE: api/owners/5
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

        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.OwnerId == id);
        }
    }
}
