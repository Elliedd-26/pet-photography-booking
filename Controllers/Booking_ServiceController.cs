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

        /// <summary>
        /// Retrieves a list of all booking-service relationships.
        /// </summary>
        /// <returns>
        /// 200 OK with a list of BookingServiceDTO
        /// </returns>
        /// <example>
        /// GET: api/Booking_Service
        /// </example>
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
        
        /// <summary>
        /// Retrieves a specific booking-service relationship by booking ID and service ID.
        /// </summary>
        /// <param name="bookingId">The ID of the booking</param>
        /// <param name="serviceId">The ID of the service</param>
        /// <returns>
        /// 200 OK with BookingServiceDTO<br/>
        /// or<br/>
        /// 404 Not Found if the relationship does not exist
        /// </returns>
        /// <example>
        /// GET: api/Booking_Service/1/2
        /// </example>
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

        /// <summary>
        /// Creates a new booking-service relationship.
        /// </summary>
        /// <param name="dto">The BookingServiceDTO containing BookingId and ServiceId</param>
        /// <returns>
        /// 201 Created with a reference to the new resource<br/>
        /// or<br/>
        /// 400 Bad Request if input is invalid
        /// </returns>
        /// <example>
        /// POST: api/Booking_Service
        /// Request Body: { "bookingId": 1, "serviceId": 2 }
        /// </example>
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

        /// <summary>
        /// Deletes an existing booking-service relationship by booking ID and service ID.
        /// </summary>
        /// <param name="bookingId">The ID of the booking</param>
        /// <param name="serviceId">The ID of the service</param>
        /// <returns>
        /// 204 No Content if deletion is successful<br/>
        /// or<br/>
        /// 404 Not Found if the relationship does not exist
        /// </returns>
        /// <example>
        /// DELETE: api/Booking_Service/1/2
        /// </example>
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
