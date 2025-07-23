using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.DTOs;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Booking_ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Booking_ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingServiceDTO>>> GetAll()
        {
            var result = await _context.Booking_Services
                .Select(bs => new BookingServiceDTO
                {
                    BookingId = bs.BookingId,
                    ServiceId = bs.ServiceId
                }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{bookingId}/{serviceId}")]
        public async Task<ActionResult<BookingServiceDTO>> Get(int bookingId, int serviceId)
        {
            var bs = await _context.Booking_Services.FindAsync(bookingId, serviceId);
            if (bs == null) return NotFound();

            return new BookingServiceDTO
            {
                BookingId = bs.BookingId,
                ServiceId = bs.ServiceId
            };
        }

        [HttpPost]
        public async Task<ActionResult> Create(BookingServiceDTO dto)
        {
            var entity = new Booking_Service
            {
                BookingId = dto.BookingId,
                ServiceId = dto.ServiceId
            };

            _context.Booking_Services.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { bookingId = dto.BookingId, serviceId = dto.ServiceId }, dto);
        }

        [HttpDelete("{bookingId}/{serviceId}")]
        public async Task<IActionResult> Delete(int bookingId, int serviceId)
        {
            var entity = await _context.Booking_Services.FindAsync(bookingId, serviceId);
            if (entity == null) return NotFound();

            _context.Booking_Services.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
