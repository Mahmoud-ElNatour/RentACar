using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using AutoMapper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Web.Models;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Options;
using RentACar.Application.Services;

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
        private readonly EmployeeManager _employeeManager;
        private readonly DriverManager _driverManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingController> _logger;
        private readonly AuditLogManager _auditLogManager;
        private readonly DriverFeeOptions _driverFeeOptions;

        public BookingController(
            BookingManager bookingManager,
            PaymentManager paymentManager,
            CarManager carManager,
            CustomerManager customerManager,
            PromocodeManager promocodeManager,
            EmployeeManager employeeManager,
            DriverManager driverManager,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            ILogger<BookingController> logger,
            AuditLogManager auditLogManager,
            IOptions<DriverFeeOptions> driverFeeOptions)
        {
            _bookingManager = bookingManager;
            _paymentManager = paymentManager;
            _carManager = carManager;
            _customerManager = customerManager;
            _promocodeManager = promocodeManager;
            _employeeManager = employeeManager;
            _driverManager = driverManager;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _auditLogManager = auditLogManager;
            _driverFeeOptions = driverFeeOptions.Value;
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

            ViewBag.DriverFeeRate = _driverFeeOptions.Rate;
            ViewBag.DriverFeeMode = _driverFeeOptions.Mode;

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

        [HttpGet("~/Booking/Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            var editDto = _mapper.Map<BookingEditDto>(booking);
            editDto.BookingStatus = "Booked"; // Setting status to Booked
            
            await _bookingManager.UpdateBookingAsync(editDto);

            // Redirect to Edit page as requested ("took me the edit page of this booking")
            return RedirectToAction("Edit", new { id = booking.BookingId }); 
        }

        [HttpGet("~/Booking/Details/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            var customer = await _customerManager.GetCustomerById(booking.CustomerId);
            var car = await _carManager.GetCarByIdAsync(booking.CarId);
            var employee = booking.EmployeebookerId.HasValue
                ? await _employeeManager.GetEmployeeById(booking.EmployeebookerId.Value)
                : null;
            var payments = await _paymentManager.GetPaymentsByBookingIdAsync(booking.BookingId);
            var payment = payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .FirstOrDefault();
            var promo = booking.PromocodeId.HasValue
                ? await _promocodeManager.GetPromocodeByIdAsync(booking.PromocodeId.Value)
                : null;
            var driver = booking.DriverId.HasValue
                ? (await _driverManager.GetAllDriversAsync()).FirstOrDefault(d => d.DriverId == booking.DriverId.Value)
                : null;

            var viewModel = new BookingDetailsViewModel
            {
                BookingId = booking.BookingId,
                BookingStatus = booking.BookingStatus,
                StartDate = booking.Startdate,
                EndDate = booking.Enddate,
                TotalPrice = booking.TotalPrice,
                Subtotal = booking.Subtotal,
                CustomerName = customer?.Name,
                CustomerUsername = customer?.username,
                CustomerEmail = customer?.Email,
                CustomerPhone = customer?.PhoneNumber,
                EmployeeName = employee?.Name,
                DriverName = driver?.DisplayName,
                DriverPhone = driver?.PhoneNumber,
                HasDriver = booking.HasDriver,
                DriverFee = booking.DriverFee,
                PickupAddress = booking.PickupAddress,
                PickupLat = booking.PickupLat,
                PickupLng = booking.PickupLng,
                CarModel = car?.ModelName,
                CarPlateNumber = car?.PlateNumber,
                CarCategory = car?.CategoryName,
                CarColor = car?.Color,
                CarModelYear = car?.ModelYear,
                CarPricePerDay = car?.PricePerDay,
                PaymentId = payment?.PaymentId,
                PaymentAmount = payment?.Amount,
                PromocodeName = promo?.Name,
                PromocodeDiscount = promo?.DiscountPercentage
            };

            return PartialView("~/Views/ControlPanel/Booking/_BookingDetailsPartial.cshtml", viewModel);

        }

        [HttpGet("~/Booking/{id}/AvailableDrivers")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AvailableDrivers(int id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var drivers = await _driverManager.GetAvailableDriversAsync(booking.Startdate, booking.Enddate);
            var result = drivers.Select(d => new
            {
                driverId = d.DriverId,
                name = d.DisplayName,
                phone = d.PhoneNumber
            });

            return Ok(result);
        }

        [HttpPost("~/Booking/AssignDriver")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AssignDriver([FromBody] AssignDriverRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booking = await _bookingManager.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound();
            }

            var availableDrivers = await _driverManager.GetAvailableDriversAsync(booking.Startdate, booking.Enddate);
            if (request.DriverId.HasValue && !availableDrivers.Any(d => d.DriverId == request.DriverId.Value))
            {
                return BadRequest("Selected driver is not available for this booking timeframe.");
            }

            var editDto = _mapper.Map<BookingEditDto>(booking);
            editDto.DriverId = request.DriverId;
            editDto.BookingStatus = booking.BookingStatus ?? "Pending";

            var updated = await _bookingManager.UpdateBookingAsync(editDto);
            if (updated == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to assign driver.");
            }

            var summary = request.DriverId.HasValue
                ? $"Assigned driver {request.DriverId.Value} to booking {request.BookingId}."
                : $"Unassigned driver from booking {request.BookingId}.";

            await _auditLogManager.LogEventAsync("Booking.DriverAssignmentChanged", "Booking", request.BookingId.ToString(), summary, null, "Success");

            return Ok(new { success = true });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            var bookings = await _bookingManager.GetAllBookingsAsync();

            var result = new List<object>(bookings.Count);
            foreach (var b in bookings)
            {
                var customer = await _customerManager.GetCustomerById(b.CustomerId);
                var car = await _carManager.GetCarByIdAsync(b.CarId);
                var employee = b.EmployeebookerId.HasValue
                    ? await _employeeManager.GetEmployeeById(b.EmployeebookerId.Value)
                    : null;
                var payments = await _paymentManager.GetPaymentsByBookingIdAsync(b.BookingId);
                var payment = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenByDescending(p => p.PaymentId)
                    .FirstOrDefault();
                var promo = b.PromocodeId.HasValue
                    ? await _promocodeManager.GetPromocodeByIdAsync(b.PromocodeId.Value)
                    : null;


                result.Add(new
                {
                    bookingId = b.BookingId,
                    customerId = b.CustomerId,
                    customerName = customer?.Name,
                    customerUsername = customer?.username,
                    customerEmail = customer?.Email,
                    carId = b.CarId,
                    carModel = car?.ModelName,
                    carPlate = car?.PlateNumber,
                    carYear = car?.ModelYear,
                    carColor = car?.Color,
                    carPricePerDay = car?.PricePerDay,
                    employeebookerId = b.EmployeebookerId,
                    employeeName = employee?.Name,
                    paymentId = payment?.PaymentId,
                    paymentAmount = payment?.Amount,
                    subtotal = b.Subtotal,
                    totalPrice = b.TotalPrice,
                    promocodeId = b.PromocodeId,
                    promocodeName = promo?.Name,
                    promocodeDiscount = promo?.DiscountPercentage,
                    startdate = b.Startdate.ToString("yyyy-MM-dd"),
                    enddate = b.Enddate.ToString("yyyy-MM-dd"),
                    bookingStatus = b.BookingStatus
                });
            }

            return Ok(result);
        }

        [HttpPost("Pay")]
        public async Task<IActionResult> Pay([FromBody] MakePaymentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var customer = await _customerManager.GetCustomerByAspNetUserId(userId);
            if (customer == null)
            {
                return Unauthorized();
            }

            var result = await _paymentManager.MakePaymentByCustomerAsync(dto, customer.UserId);
            if (result == null)
            {
                return BadRequest("Unable to process payment request.");
            }

            if (result.RequiresRedirect && !string.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                return Redirect(result.RedirectUrl);
            }

            return Ok(result.Payment);
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
        public async Task<ActionResult<BookingCreationResultDto>> Create([FromBody] MakeBookingRequestDto dto)
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

                return CreatedAtAction(nameof(Get), new { id = created.Booking.BookingId }, created);
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
            try
            {
                var success = await _bookingManager.DeleteBookingAsync(new DeleteBookingRequestDto { BookingId = id });
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint prevented deleting booking {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Unable to delete booking because related records exist. Remove the related data before deleting the booking.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting booking {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while deleting the booking. Please try again later.");
            }
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
            var payments = await _paymentManager.GetPaymentsByBookingIdAsync(booking.BookingId);
            payment = payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .FirstOrDefault();
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
                            row.RelativeItem().Text("Customer Signature: ___________________");
                            row.RelativeItem().AlignRight().Text("Company Signature: ___________________");
                        });
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}
