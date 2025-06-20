using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Managers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Employee,Customer")]
    [Route("api/[controller]")]
    public class BookingController : Controller
    {
        private readonly BookingManager _bookingManager;
        private readonly PaymentManager _paymentManager;
        private readonly CarManager _carManager;
        private readonly CustomerManager _customerManager;
        private readonly PromocodeManager _promocodeManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmployeeManager _employeeManager;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            BookingManager bookingManager,
            PaymentManager paymentManager,
            CarManager carManager,
            CustomerManager customerManager,
            PromocodeManager promocodeManager,
            UserManager<IdentityUser> userManager,
            EmployeeManager employeeManager,
            ILogger<BookingController> logger)
        {
            _bookingManager = bookingManager;
            _paymentManager = paymentManager;
            _carManager = carManager;
            _customerManager = customerManager;
            _promocodeManager = promocodeManager;
            _userManager = userManager;
            _employeeManager = employeeManager;
            _logger = logger;
        }

        [HttpGet("~/Booking")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Booking/Index.cshtml");
        }

        [HttpGet("~/Booking/Add")]
        //[Authorize(Roles = "Admin,Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Add(int? carId = null)
        {
            if (carId.HasValue)
            {
                var (start, end) = await _bookingManager.SuggestBookingDatesAsync(carId.Value);
                ViewBag.StartDate = start.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
                ViewBag.EndDate = end.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
                ViewBag.CarId = carId.Value.ToString();
            }

            return View("~/Views/ControlPanel/Booking/Add.cshtml", new BookingDto());
        }

        [HttpGet("~/Booking/Edit/{id}")]
       // [Authorize(Roles = "Admin,Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            return View("~/Views/ControlPanel/Booking/Edit.cshtml", booking);
        }

        [HttpGet("~/Booking/Delete/{id}")]
        //[Authorize(Roles = "Admin,Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            return View("~/Views/ControlPanel/Booking/Delete.cshtml", booking);
        }

        //used in tablw when viewing all
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            var bookings = await _bookingManager.GetAllBookingsAsync();

            var result = new List<object>();
            foreach (var b in bookings)
            {
                decimal? paymentAmount = null;
                if (b.PaymentId.HasValue)
                {
                    var payment = await _paymentManager.GetPaymentByIdAsync(b.PaymentId.Value);
                    paymentAmount = payment?.Amount;
                }

                var cutmomerName = _customerManager.GetCustomerById(b.CustomerId).Result.Name;

                var employeeName = "";
                if (b.EmployeebookerId.HasValue)
                {
                    employeeName = _employeeManager.GetEmployeeById((int)(b.EmployeebookerId)).Result.Name;
                }

                var carName = _carManager.GetCarByIdAsync(b.CarId).Result.ModelName;

                result.Add(new
                {
                    bookingId = b.BookingId,
                    customerId = b.CustomerId,
                    carId = b.CarId,
                    employeebookerId = b.EmployeebookerId,
                    paymentId = b.PaymentId,
                    paymentAmount,
                    cutmomerName,
                    employeeName,
                    carName,
                    subtotal = b.Subtotal,
                    promocodeId = b.PromocodeId,
                    startdate = b.Startdate.ToString("yyyy-MM-dd"),
                    enddate = b.Enddate.ToString("yyyy-MM-dd"),
                    bookingStatus = b.BookingStatus
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> Get(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create([FromBody] MakeBookingRequestDto dto)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                // Log and return detailed validation errors
                _logger.LogWarning("Invalid booking DTO: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Creating booking by user {UserId} with data: {@Dto}", userId, dto);

            try
            {
                var created = await _bookingManager.MakeBookingAsync(dto, userId);

                if (created == null)
                {
                    _logger.LogWarning("BookingManager returned null for user {UserId}", userId);
                    return BadRequest("Booking could not be created. Please check availability, customer status, or payment info.");
                }

                return CreatedAtAction(nameof(Get), new { id = created.BookingId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while creating booking");
                return StatusCode(500, "Internal server error while processing booking.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BookingEditDto dto)
        {
            if (id != dto.BookingId)
                return BadRequest();

            var updated = await _bookingManager.UpdateBookingAsync(dto);
            if (updated == null)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bookingManager.DeleteBookingAsync(new DeleteBookingRequestDto { BookingId = id });
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("~/Booking/Contract/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Contract(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            var car = await _carManager.GetCarByIdAsync(booking.CarId);
            var customer = await _customerManager.GetCustomerById(booking.CustomerId);
            PaymentDto? payment = null;
            if (booking.PaymentId.HasValue)
                payment = await _paymentManager.GetPaymentByIdAsync(booking.PaymentId.Value);
            PromocodeDto? promo = null;
            if (booking.PromocodeId.HasValue)
                promo = await _promocodeManager.GetPromocodeByIdAsync(booking.PromocodeId.Value);

            var bytes = GenerateContractPdf(booking, car, customer, payment, promo);
            return File(bytes, "application/pdf", $"booking_contract_{id}.pdf");
        }

        private byte[] GenerateContractPdf(BookingDto booking, CarDto? car, CustomerDTO? customer, PaymentDto? payment, PromocodeDto? promo)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("Rental Contract").FontSize(20).Bold();
                        col.Item().Text($"Booking ID: {booking.BookingId}");
                        if (customer != null)
                            col.Item().Text($"Customer: {customer.Name} (ID: {customer.UserId})");
                        if (car != null)
                            col.Item().Text($"Car: {car.ModelName} - {car.PlateNumber}");
                        if (payment != null)
                            col.Item().Text($"Payment ID: {payment.PaymentId} Amount: {payment.Amount:C}");
                        if (promo != null)
                            col.Item().Text($"Promocode: {promo.Name} ({promo.DiscountPercentage}% off)");
                        col.Item().Text($"Start Date: {booking.Startdate:yyyy-MM-dd}");
                        col.Item().Text($"End Date: {booking.Enddate:yyyy-MM-dd}");
                        if (booking.Subtotal != null)
                            col.Item().Text($"Subtotal: {booking.Subtotal:C}");
                        else
                            col.Item().Text($"Total Price: {booking.TotalPrice:C}");
                        col.Item().PaddingVertical(20).Text("I, the renter, accept responsibility for the rental vehicle.");
                        col.Item().Row(row =>
                        {
                            row.RelativeColumn().Text("Customer Signature: ___________________");
                            row.RelativeColumn().AlignRight().Text("Company Signature: ___________________");
                        });
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}
