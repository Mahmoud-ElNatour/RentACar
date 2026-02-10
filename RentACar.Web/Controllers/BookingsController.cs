using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Linq;
using RentACar.Application.Services;

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
            UserManager<IdentityUser> userManager,
            CustomerRatingManager ratingManager)
        {
            _bookingManager = bookingManager;
            _paymentManager = paymentManager;
            _carManager = carManager;
            _customerManager = customerManager;
            _userManager = userManager;
            _ratingManager = ratingManager;
        }

        private readonly CustomerRatingManager _ratingManager;

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
            var ratings = await _ratingManager.GetRatingsByUserIdAsync(customerId.Value);
            var ratedBookingIds = ratings.Select(r => r.BookingId).ToHashSet();

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
                    employeeId = b.EmployeebookerId,
                    bookingStatus = b.BookingStatus,
                    hasDriver = b.HasDriver,
                    isRated = ratedBookingIds.Contains(b.BookingId)
                });
            }
            return Ok(result);
        }

        [HttpPost("Pay")]
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
                        var pricing = BookingPricingCalculator.Calculate(
                            car?.PricePerDay ?? 0m,
                            booking.Startdate,
                            booking.Enddate,
                            booking.HasDriver,
                            booking.DriverDailyFee,
                            car?.ExtraDriverFeePerDay,
                            null);

                        col.Item().Text($"Car Rental: {pricing.BaseRental:C}");
                        if (pricing.DriverService > 0)
                        {
                            col.Item().Text($"Driver Service: {pricing.DriverService:C}");
                        }
                        if (pricing.CarExtraDriverFee > 0)
                        {
                            col.Item().Text($"Car Extra Driver Fee: {pricing.CarExtraDriverFee:C}");
                        }
                        var subtotal = booking.Subtotal ?? pricing.Subtotal;
                        var discount = subtotal - booking.TotalPrice;
                        col.Item().Text($"Subtotal: {subtotal:C}");
                        if (discount > 0)
                        {
                            col.Item().Text($"Discount: -{discount:C}");
                        }
                        col.Item().Text($"Total Price: {booking.TotalPrice:C}");
                    });
                });
            });
            return document.GeneratePdf();
        }
        [HttpGet("~/Bookings/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != customerId.Value) return NotFound();

            var car = await _carManager.GetCarByIdAsync(booking.CarId);
            var payments = await _paymentManager.GetPaymentsByBookingIdAsync(booking.BookingId);
            var payment = payments.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.PaymentId).FirstOrDefault();

            // Re-using the same DTO as admin panel is fine since it's just data transfer
            var dto = new BookingDetailsDto
            {
                BookingId = booking.BookingId,
                BookingStatus = booking.BookingStatus,
                StartDate = booking.Startdate,
                EndDate = booking.Enddate,
                TotalPrice = booking.TotalPrice,
                Subtotal = booking.Subtotal,
                CarModel = car?.ModelName,
                CarPlateNumber = car?.PlateNumber,
                CarCategory = car?.CategoryName,
                CarColor = car?.Color,
                CarModelYear = car?.ModelYear,
                CarPricePerDay = car?.PricePerDay,
                CarImageUrl = car?.CarImage != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(car.CarImage)}" : null,
                PaymentId = payment?.PaymentId,
                PaymentAmount = payment?.Amount
            };

            // Rating Login
            var rating = await _ratingManager.GetRatingByBookingIdAsync(booking.BookingId);
            if (rating != null)
            {
                dto.IsRated = true;
                dto.VerifiedRatingStars = rating.Stars;
                dto.RatingFeedback = rating.Feedback;
            }
            else
            {
                dto.IsRated = false;
            }

            return PartialView("~/Views/Bookings/_CustomerBookingDetailsPartial.cshtml", dto);
        }

        [HttpPost("Rate")]
        public async Task<IActionResult> Rate([FromBody] RateBookingRequestDto request)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var booking = await _bookingManager.GetBookingByIdAsync(request.BookingId);
            if (booking == null || booking.CustomerId != customerId.Value) return NotFound();

            // Check timing
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (booking.Startdate > today)
            {
                return BadRequest("You can only rate a booking after it has started.");
            }

            if (booking.BookingStatus == "Cancelled")
            {
                return BadRequest("You cannot rate a cancelled booking.");
            }

            // Check if already rated
            var existing = await _ratingManager.GetRatingByBookingIdAsync(request.BookingId);
            if (existing != null)
            {
                return BadRequest("This booking has already been rated.");
            }

            var ratingId = await _ratingManager.AddRatingAsync(customerId.Value, request.BookingId, request.Stars, request.Feedback);

            // Verify persistence
            var check = await _ratingManager.GetRatingByBookingIdAsync(request.BookingId);
            if (check == null)
            {
                return StatusCode(500, "Rating could not be saved due to an internal error.");
            }

            return Ok(new { success = true, ratingId = ratingId });
        }
        [HttpGet("~/Bookings/Receipt/{id}")]
        public async Task<IActionResult> Receipt(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();

            var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
            if (payment == null) return NotFound();

            // Verify ownership via Booking
            var booking = await _bookingManager.GetBookingByIdAsync(payment.BookingId);
            if (booking == null || booking.CustomerId != customerId.Value) return NotFound();

            return View("~/Views/ControlPanel/Payment/Receipt.cshtml", payment);
        }
    }
}
