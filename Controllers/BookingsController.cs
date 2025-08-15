using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;
using PetPhotographyApp.DTOs;

namespace PetPhotographyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of pet photography bookings in the system.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Bookings/List -> [{"BookingId":1,"OwnerName":"Lisa","PetName":"Fluffy","PhotographerName":"Jim","Services":[{"ServiceId":1,"Name":"Pet Portrait"}]}, ...]
        /// </example>
        /// <returns>
        /// A list of BookingSummaryDTO objects with related owner, pet, photographer, and service information.
        /// </returns>
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> ListBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .ToListAsync();

            var bookingSummaries = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",
                PetName = b.Pet?.Name ?? "Unknown Pet",                     // 添加了PetName
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
        
            }).ToList();

            return Ok(bookingSummaries);
        }

        /// <summary>
        /// Returns a pet photography booking specified by ID.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Bookings/Find/5 -> {"BookingId":5,"OwnerName":"Lisa","PetName":"Fluffy","PhotographerName":"Jim","Services":[{"ServiceId":1,"Name":"Pet Portrait"}]}
        /// </example>
        /// <returns>
        /// A single BookingDTO object if found; otherwise, NotFound.
        /// </returns>
        [HttpGet("Find/{id}")]
        public async Task<ActionResult<BookingDTO>> FindBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            var bookingDto = new BookingDTO
            {
                BookingId = booking.BookingId,
                BookingDate = booking.BookingDate,
                Location = booking.Location,
                OwnerId = booking.OwnerId,
                OwnerName = booking.Owner.Name,
                PetId = booking.PetId,
                PetName = booking.Pet.Name,
                PhotographerId = booking.PhotographerId,
                PhotographerName = booking.Photographer.Name,
                Services = booking.BookingServices.Select(bs => new ServiceDTO
                {
                    ServiceId = bs.ServiceId,
                    Name = bs.Service.Name,
                    Price = bs.Service.Price
                }).ToList()
            };

            return Ok(bookingDto);
        }

        /// <summary>
        /// Updates an existing pet photography booking in the system.
        /// </summary>
        /// <example>
        /// PUT http://localhost:5000/api/Bookings/Update/5
        /// </example>
        /// <returns>
        /// Returns NoContent on success, BadRequest if IDs mismatch, or NotFound if booking or related entities do not exist.
        /// </returns>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, BookingDTO bookingDto)
        {
            if (id != bookingDto.BookingId)
                return BadRequest();

            var owner = await _context.Owners.FindAsync(bookingDto.OwnerId);
            var pet = await _context.Pets.FindAsync(bookingDto.PetId);
            var photographer = await _context.Photographers.FindAsync(bookingDto.PhotographerId);

            if (owner == null || pet == null || photographer == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            // Update booking fields
            booking.BookingDate = bookingDto.BookingDate;
            booking.Location = bookingDto.Location;
            booking.OwnerId = bookingDto.OwnerId;
            booking.PetId = bookingDto.PetId;
            booking.PhotographerId = bookingDto.PhotographerId;

            // Update services
            _context.Booking_Services.RemoveRange(booking.BookingServices);

            booking.BookingServices = bookingDto.Services.Select(s => new Booking_Service
            {
                BookingId = id,
                ServiceId = s.ServiceId
            }).ToList();

            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Adds a new pet photography booking to the system.
        /// </summary>
        /// <example>
        /// POST http://localhost:5000/api/Bookings/Add
        /// </example>
        /// <returns>
        /// Returns Created with the new booking if successful, or NotFound if related entities do not exist.
        /// </returns>
        [HttpPost("Add")]
        public async Task<ActionResult<Booking>> AddBooking(BookingDTO bookingDto)
        {
            var owner = await _context.Owners.FindAsync(bookingDto.OwnerId);
            var pet = await _context.Pets.FindAsync(bookingDto.PetId);
            var photographer = await _context.Photographers.FindAsync(bookingDto.PhotographerId);

            if (owner == null || pet == null || photographer == null)
                return NotFound();

            var booking = new Booking
            {
                BookingDate = bookingDto.BookingDate,
                Location = bookingDto.Location,
                OwnerId = bookingDto.OwnerId,
                PetId = bookingDto.PetId,
                PhotographerId = bookingDto.PhotographerId
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            if (bookingDto.Services != null)
            {
                foreach (var serviceDto in bookingDto.Services)
                {
                    _context.Booking_Services.Add(new Booking_Service
                    {
                        BookingId = booking.BookingId,
                        ServiceId = serviceDto.ServiceId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("FindBooking", new { id = booking.BookingId }, bookingDto);
        }

        /// <summary>
        /// Returns all bookings for a given photographer.
        /// </summary>
        /// <example>
        /// GET http://localhost:5000/api/Bookings/BookingsForPhotographer/7
        /// </example>
        /// <param name="photographerId">The ID of the photographer.</param>
        /// <returns>
        /// A list of BookingSummaryDTO objects, or 404 if none found.
        /// </returns>
        [HttpGet("BookingsForPhotographer/{photographerId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> ListBookingsForPhotographer(int photographerId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.PhotographerId == photographerId)
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                .ToListAsync();

            if (!bookings.Any())
                return NotFound($"No bookings found for photographer ID {photographerId}.");

            var bookingSummaries = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",
                PetName = b.Pet?.Name ?? "Unknown Pet",
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
                
            }).ToList();

            return Ok(bookingSummaries);
        }

        /// <summary>
        /// Returns all services for a given booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>A list of ServiceDTO objects or NotFound if booking not found.</returns>
        [HttpGet("ServicesForBooking/{bookingId}")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> ListServicesForBooking(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound($"Booking with ID {bookingId} not found.");

            var services = booking.BookingServices.Select(bs => new ServiceDTO
            {
                ServiceId = bs.ServiceId,
                Name = bs.Service.Name,
                Price = bs.Service.Price
            }).ToList();

            return Ok(services);
        }

        /// <summary>
        /// Returns all bookings linked to a specific service.
        /// </summary>
        /// <param name="serviceId">The ID of the service.</param>
        /// <returns>A list of BookingSummaryDTO objects or NotFound if none found.</returns>
        [HttpGet("BookingsForService/{serviceId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> ListBookingsForService(int serviceId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Photographer)
                .Include(b => b.BookingServices)
                .Where(b => b.BookingServices.Any(bs => bs.ServiceId == serviceId))
                .ToListAsync();

            if (!bookings.Any())
                return NotFound($"No bookings found for service ID {serviceId}.");

            var bookingSummaries = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                Location = b.Location ?? string.Empty,
                OwnerName = b.Owner?.Name ?? "Unknown Owner",
                PetName = b.Pet?.Name ?? "Unknown Pet",
                PhotographerName = b.Photographer?.Name ?? "Unknown Photographer",
        
            }).ToList();

            return Ok(bookingSummaries);
        }

        /// <summary>
        /// Deletes a pet photography booking from the system.
        /// </summary>
        /// <example>
        /// DELETE http://localhost:5000/api/Bookings/Delete/5
        /// </example>
        /// <returns>
        /// Returns NoContent on success or NotFound if booking does not exist.
        /// </returns>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            _context.Booking_Services.RemoveRange(booking.BookingServices);

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(b => b.BookingId == id);
        }
    }
}