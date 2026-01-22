using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Linq;
using RentACar.Core.Repositories;
using RentACar.Web.Models;

namespace RentACar.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : Controller
    {
        private readonly BookingManager _bookingManager;
        private readonly PaymentManager _paymentManager;
        private readonly CarManager _carManager;
        private readonly CustomerManager _customerManager;
        private readonly DriverManager _driverManager;
        private readonly IDriverLocationRepository _driverLocationRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(
            BookingManager bookingManager,
            PaymentManager paymentManager,
            CarManager carManager,
            CustomerManager customerManager,
            DriverManager driverManager,
            IDriverLocationRepository driverLocationRepository,
            UserManager<IdentityUser> userManager)
        {
            _bookingManager = bookingManager;
            _paymentManager = paymentManager;
            _carManager = carManager;
            _customerManager = customerManager;
            _driverManager = driverManager;
            _driverLocationRepository = driverLocationRepository;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentCustomerId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var customer = await _customerManager.GetCustomerByUsername(user.UserName!);
            return customer?.UserId;
        }

        [HttpGet("~/Bookings/MyBookings")]
        [Authorize(Roles = "Customer")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult MyBookings()
        {
            return View("~/Views/Bookings/MyBookings.cshtml");
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Get()
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var bookings = await _bookingManager.GetBookingHistoryAsync(customerId.Value);
            var result = new List<object>();
            foreach (var b in bookings)
            {
                var car = await _carManager.GetCarByIdAsync(b.CarId);
                var payments = await _paymentManager.GetPaymentsByBookingIdAsync(b.BookingId);
                var latestPayment = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenByDescending(p => p.PaymentId)
                    .FirstOrDefault();
                result.Add(new
                {
                    bookingId = b.BookingId,
                    carName = car?.ModelName,
                    plateNumber = car?.PlateNumber,
                    paymentId = latestPayment?.PaymentId,
                    paymentStatus = latestPayment?.Status,
                    startdate = b.Startdate.ToString("yyyy-MM-dd"),
                    enddate = b.Enddate.ToString("yyyy-MM-dd"),
                    totalPrice = b.TotalPrice,
                    hasDriver = b.HasDriver,
                    driverId = b.DriverId
                });
            }
            return Ok(result);
        }

        [HttpPost("Pay")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Pay([FromBody] MakePaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var result = await _paymentManager.MakePaymentByCustomerAsync(request, customerId.Value);
            if (result == null)
            {
                return BadRequest("Unable to process payment.");
            }

            return Ok(result);
        }

        [HttpGet("~/Bookings/Ticket/{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Ticket(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != customerId.Value) return NotFound();

            var car = await _carManager.GetCarByIdAsync(booking.CarId);
            PaymentDto? payment = null;
            var payments = await _paymentManager.GetPaymentsByBookingIdAsync(booking.BookingId);
            payment = payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .FirstOrDefault();

            var bytes = GenerateTicketPdf(booking, car, payment);
            return File(bytes, "application/pdf", $"booking_{id}.pdf");
        }

        [HttpGet("~/Bookings/{id}/TrackDriver")]
        [Authorize(Roles = "Customer,Admin,Employee")]
        public async Task<IActionResult> TrackDriver(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isEmployee = User.IsInRole("Employee") || User.IsInRole("Admin");
            if (!isEmployee)
            {
                var customer = await _customerManager.GetCustomerByAspNetUserId(userId);
                if (customer == null || booking.CustomerId != customer.UserId)
                {
                    return Forbid();
                }
            }

            var driver = booking.DriverId.HasValue
                ? (await _driverManager.GetAllDriversAsync()).FirstOrDefault(d => d.DriverId == booking.DriverId.Value)
                : null;
            var location = await _driverLocationRepository.GetLatestByBookingIdAsync(booking.BookingId);

            var viewModel = new TrackDriverViewModel
            {
                BookingId = booking.BookingId,
                DriverName = driver?.DisplayName,
                DriverPhone = driver?.PhoneNumber,
                DriverLat = location?.Latitude,
                DriverLng = location?.Longitude,
                LastUpdatedUtc = location?.LastUpdatedUtc,
                PickupAddress = booking.PickupAddress,
                PickupLat = booking.PickupLat,
                PickupLng = booking.PickupLng,
                IsTrackingActive = location?.IsTrackingActive ?? false
            };

            return View("~/Views/Bookings/TrackDriver.cshtml", viewModel);
        }

        private byte[] GenerateTicketPdf(BookingDto booking, CarDto? car, PaymentDto? payment)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A5);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Booking Ticket").FontSize(20).Bold();
                        col.Item().Text($"Booking ID: {booking.BookingId}");
                        if (car != null)
                            col.Item().Text($"Car: {car.ModelName} - {car.PlateNumber}");
                        if (payment != null)
                            col.Item().Text($"Payment ID: {payment.PaymentId}");
                        col.Item().Text($"Start Date: {booking.Startdate:yyyy-MM-dd}");
                        col.Item().Text($"End Date: {booking.Enddate:yyyy-MM-dd}");
                        col.Item().Text($"Total Price: {booking.TotalPrice:C}");
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}
