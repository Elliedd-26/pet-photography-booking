using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.DTOs;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers
{
    public class OwnersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // View page — return ViewModel (DTO)
        public async Task<IActionResult> Index()
        {
            var owners = await _context.Owners
                .Include(o => o.Pets)
                .ToListAsync();

            var ownerDTOs = owners.Select(o => new OwnerDTO
            {
                OwnerId = o.OwnerId,
                Name = o.Name,
                Email = o.Email ?? "",
                PhoneNumber = o.PhoneNumber ?? "",
                Pets = o.Pets.Select(p => new PetDTO
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Species = p.Species ?? "",
                    Breed = p.Breed,
                    Age = p.Age ?? 0,
                    Description = p.Description,
                    OwnerName = o.Name
                }).ToList()
            }).ToList();

            return View(ownerDTOs); // 你需要在 View 中对应用 OwnerDTO 类型
        }

        // JSON API — optional, if you want JSON data
        [HttpGet("api/owners")]
        public async Task<IActionResult> GetAllOwners()
        {
            var owners = await _context.Owners
                .Include(o => o.Pets)
                .ToListAsync();

            var ownerDTOs = owners.Select(o => new OwnerDTO
            {
                OwnerId = o.OwnerId,
                Name = o.Name,
                Email = o.Email ?? "",
                PhoneNumber = o.PhoneNumber ?? "",
                Pets = o.Pets.Select(p => new PetDTO
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Species = p.Species ?? "",
                    Breed = p.Breed,
                    Age = p.Age ?? 0,
                    Description = p.Description,
                    OwnerName = o.Name
                }).ToList()
            });

            return Ok(ownerDTOs);
        }
    }
}
