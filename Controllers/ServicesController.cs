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

        /// <summary>
        /// Returns a list of active services in the system.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Services/List 
        /// -> [{"ServiceId":1,"Name":"Pet Portrait","Price":200.0},{"ServiceId":2,"Name":"Pet Birthday Shoot","Price":180.0}]
        /// </example>
        /// <returns>
        /// A list of ServiceDTO objects.
        /// </returns>
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


        /// <summary>
        /// Returns a single service by ID.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Services/Find/2 
        /// -> {"ServiceId":2,"Name":"Pet Birthday Shoot","Price":180.0}
        /// </example>
        /// <returns>
        /// A ServiceDTO object if found; otherwise, 404 Not Found.
        /// </returns>
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

        /// <summary>
        /// Adds a new service to the system.
        /// </summary>
        /// <example>
        /// POST http://localhost:5000/api/Services/Add  
        /// Body: {"Name":"Holiday Special","Price":250.0}
        /// </example>
        /// <returns>
        /// The newly created ServiceDTO object with location header.
        /// </returns>
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

       /// <summary>
        /// Updates an existing service in the system.
        /// </summary>
        /// <example>
        /// PUT http://localhost:5000/api/Services/Update/3  
        /// Body: {"ServiceId":3,"Name":"Updated Service","Price":275.0}
        /// </example>
        /// <returns>
        /// NoContent on success; BadRequest if ID mismatch; NotFound if service does not exist.
        /// </returns>
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

        /// <summary>
        /// Deletes a service by ID. Deactivates if it has active bookings.
        /// </summary>
        /// <example>
        /// DELETE http://localhost:5000/api/Services/Delete/3
        /// </example>
        /// <returns>
        /// NoContent on success; NotFound if service does not exist.
        /// </returns>
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


        /// <summary>
        /// Lists bookings associated with a specific service.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Services/ListBookingsByService/2
        /// </example>
        /// <param name="serviceId">ID of the service.</param>
        /// <returns>
        /// A list of BookingDTO objects for bookings related to the specified service.
        /// </returns>
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


        /// <summary>
        /// Returns a list of services within the specified price range.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Services/ByPriceRange?minPrice=100&amp;maxPrice=250
        /// </example>
        /// <param name="minPrice">Minimum price of the service.</param>
        /// <param name="maxPrice">Maximum price of the service.</param>
        /// <returns>
        /// A list of ServiceDTO objects that match the given price range.
        /// </returns>
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
