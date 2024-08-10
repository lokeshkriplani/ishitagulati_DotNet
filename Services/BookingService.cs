using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ishitagualti.Models;
using ishitagualti.Controllers;
using ishitagualti.Data;



public class BookingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(ApplicationDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<MakeupBooking>> GetBookingsAsync()
    {
        _logger.LogInformation("Fetching bookings from database.");

        var bookings = await _context.MakeupBookings.ToListAsync();

        if (bookings == null || !bookings.Any())
        {
            _logger.LogInformation("No bookings found in the database.");
        }
        else
        {
            _logger.LogInformation($"Found {bookings.Count} bookings in the database.");
        }

        return bookings;
    }
}
