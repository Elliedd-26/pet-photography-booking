using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.DTOs;

namespace PetPhotographyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> ListServices()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var serviceDtos = services.Select(s => new ServiceDTO
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Price = s.Price
            }).ToList();

            return Ok(serviceDtos);
        }

        [HttpGet("Find/{id}")]
        public async Task<ActionResult<ServiceDTO>> FindService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) 
                return NotFound($"Service with ID {id} not found.");

            return Ok(new ServiceDTO
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price
            });
        }

        [HttpPost("Add")]
        public async Task<ActionResult<ServiceDTO>> AddService(ServiceDTO serviceDto)
        {
            if (string.IsNullOrWhiteSpace(serviceDto.Name))
                return BadRequest("Service name is required.");

            if (serviceDto.Price < 0)
                return BadRequest("Service price cannot be negative.");

            var service = new Service
            {
                Name = serviceDto.Name.Trim(),
                Price = serviceDto.Price,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            serviceDto.ServiceId = service.ServiceId;
            return CreatedAtAction(nameof(FindService), new { id = service.ServiceId }, serviceDto);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateService(int id, ServiceDTO serviceDto)
        {
            if (id != serviceDto.ServiceId)
                return BadRequest("Service ID mismatch.");

            var service = await _context.Services.FindAsync(id);
            if (service == null) 
                return NotFound($"Service with ID {id} not found.");

            if (string.IsNullOrWhiteSpace(serviceDto.Name))
                return BadRequest("Service name is required.");

            if (serviceDto.Price < 0)
                return BadRequest("Service price cannot be negative.");

            service.Name = serviceDto.Name.Trim();
            service.Price = serviceDto.Price;
            service.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) 
                return NotFound($"Service with ID {id} not found.");

            var hasActiveBookings = await _context.Booking_Services
                .AnyAsync(bs => bs.ServiceId == id && bs.Booking.Status != "Cancelled");

            if (hasActiveBookings)
            {
                service.IsActive = false;
                service.LastModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("ListBookingsByService/{serviceId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> ListBookingsByService(int serviceId)
        {
            var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == serviceId);
            if (!serviceExists)
                return NotFound($"Service with ID {serviceId} not found.");

            var bookings = await _context.Booking_Services
                .Where(bs => bs.ServiceId == serviceId)
                .Include(bs => bs.Booking)
                    .ThenInclude(b => b.Owner)
                .Include(bs => bs.Booking)
                    .ThenInclude(b => b.Pet)
                .Include(bs => bs.Booking)
                    .ThenInclude(b => b.Photographer)
                .Include(bs => bs.Booking)
                    .ThenInclude(b => b.BookingServices)
                        .ThenInclude(bs => bs.Service)
                .Select(bs => bs.Booking)
                .Distinct()
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            if (!bookings.Any())
                return NotFound($"No bookings found for service ID {serviceId}.");

            var result = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",
                PetName = b.Pet?.Name ?? "Unknown Pet",
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
                Services = b.BookingServices.Select(s => new ServiceDTO
                {
                    ServiceId = s.Service.ServiceId,
                    Name = s.Service.Name,
                    Price = s.Service.Price
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("ByPriceRange")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByPriceRange(
            [FromQuery] decimal minPrice = 0, 
            [FromQuery] decimal maxPrice = decimal.MaxValue)
        {
            if (minPrice < 0 || maxPrice < 0)
                return BadRequest("Price values cannot be negative.");
            if (minPrice > maxPrice)
                return BadRequest("Minimum price cannot be greater than maximum price.");

            var services = await _context.Services
                .Where(s => s.IsActive && s.Price >= minPrice && s.Price <= maxPrice)
                .OrderBy(s => s.Price)
                .Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price
                })
                .ToListAsync();

            return Ok(services);
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}
