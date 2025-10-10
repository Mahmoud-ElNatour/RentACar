using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Managers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : Controller
    {
        private readonly BookingManager _bookingManager;
        private readonly PaymentManager _paymentManager;
        private readonly CarManager _carManager;
        private readonly CustomerManager _customerManager;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(
            BookingManager bookingManager,
            PaymentManager paymentManager,
            CarManager carManager,
            CustomerManager customerManager,
            UserManager<IdentityUser> userManager)
        {
            _bookingManager = bookingManager;
            _paymentManager = paymentManager;
            _carManager = carManager;
            _customerManager = customerManager;
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
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult MyBookings()
        {
            return View("~/Views/Bookings/MyBookings.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var bookings = await _bookingManager.GetBookingHistoryAsync(customerId.Value);
            var result = new List<object>();
            foreach (var b in bookings)
            {
                var car = await _carManager.GetCarByIdAsync(b.CarId);
                result.Add(new
                {
                    bookingId = b.BookingId,
                    carName = car?.ModelName,
                    plateNumber = car?.PlateNumber,
                    paymentId = b.PaymentId,
                    startdate = b.Startdate.ToString("yyyy-MM-dd"),
                    enddate = b.Enddate.ToString("yyyy-MM-dd"),
                    totalPrice = b.TotalPrice
                });
            }
            return Ok(result);
        }

        [HttpGet("~/Bookings/Ticket/{id}")]
        public async Task<IActionResult> Ticket(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != customerId.Value) return NotFound();

            var car = await _carManager.GetCarByIdAsync(booking.CarId);
            PaymentDto? payment = null;
            if (booking.PaymentId.HasValue)
                payment = await _paymentManager.GetPaymentByIdAsync(booking.PaymentId.Value);

            var bytes = GenerateTicketPdf(booking, car, payment);
            return File(bytes, "application/pdf", $"booking_{id}.pdf");
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
