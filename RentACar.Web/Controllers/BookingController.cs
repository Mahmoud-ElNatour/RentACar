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
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Web.Models;
using System.Security.Claims;
using System.Linq;

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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            BookingManager bookingManager,
            PaymentManager paymentManager,
            CarManager carManager,
            CustomerManager customerManager,
            PromocodeManager promocodeManager,
            EmployeeManager employeeManager,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            ILogger<BookingController> logger)
        {
            _bookingManager = bookingManager;
            _paymentManager = paymentManager;
            _carManager = carManager;
            _customerManager = customerManager;
            _promocodeManager = promocodeManager;
            _employeeManager = employeeManager;
            _userManager = userManager;
            _mapper = mapper;
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

            // Fetch details for pre-filling the search inputs
            var customer = await _customerManager.GetCustomerById(booking.CustomerId);
            ViewBag.CustomerName = customer?.Name ?? string.Empty;

            var car = await _carManager.GetCarByIdAsync(booking.CarId);
            ViewBag.CarName = car != null ? $"{car.ModelName} - {car.PlateNumber}" : string.Empty;

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

            var dto = new BookingDetailsDto
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
                CarModel = car?.ModelName,
                CarPlateNumber = car?.PlateNumber,
                CarCategory = car?.CategoryName,
                CarColor = car?.Color,
                CarModelYear = car?.ModelYear,
                CarPricePerDay = car?.PricePerDay,
                CarImageUrl = car?.CarImage != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(car.CarImage)}" : null,
                PaymentId = payment?.PaymentId,
                PaymentAmount = payment?.Amount,
                PromocodeName = promo?.Name,
                PromocodeDiscount = promo?.DiscountPercentage
            };

            return PartialView("~/Views/ControlPanel/Booking/_BookingDetailsPartial.cshtml", dto);

        }

        [HttpGet("GetFilteredBookings")]
        public async Task<ActionResult<PagedResultDto<BookingListDto>>> GetFilteredBookings(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? sortColumn = "BookingId",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : (DateOnly?)null;
            var end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : (DateOnly?)null;

            var result = await _bookingManager.GetBookingsPagedAsync(page, pageSize, search, status, sortColumn, sortDirection, start, end);
            return Ok(result);
        }

        [HttpGet("GetStats")]
        public async Task<ActionResult<object>> GetStats(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : (DateOnly?)null;
            var end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : (DateOnly?)null;

            var stats = await _bookingManager.GetBookingStatsAsync(search, status, start, end);
            return Ok(stats);
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

            var userId = _userManager.GetUserId(User) ?? string.Empty;
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
            var gold = "#d4af37";
            var dark = "#16181a";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    //page.Background(dark); // Contracts usually white for print, let's keep white but use Dark/Gold text.
                    
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Lebanon Drive").FontSize(24).Bold().FontColor(dark);
                            col.Item().Text("RentACar").FontSize(24).Bold().FontColor(gold);
                        });
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("RENTAL AGREEMENT").FontSize(16).SemiBold().FontColor(Colors.Grey.Medium);
                            col.Item().Text($"#{booking.BookingId}").FontSize(20).Bold().FontColor(dark);
                            col.Item().Text($"{DateTime.Now:dd MMM yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // 1. Customer & Car Section
                        col.Item().Row(row =>
                        {
                            // Customer
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("LESSEE (CUSTOMER)").FontSize(10).SemiBold().FontColor(gold);
                                if(customer != null)
                                {
                                    c.Item().Text(customer.Name).Bold().FontSize(12);
                                    c.Item().Text(customer.Email).FontSize(10);
                                    c.Item().Text(customer.PhoneNumber ?? "-").FontSize(10);
                                    c.Item().Text(customer.Address ?? "No Address Provided").FontSize(10);
                                }
                            });
                            
                            row.ConstantItem(20);

                            // Car
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("VEHICLE").FontSize(10).SemiBold().FontColor(gold);
                                if(car != null)
                                {
                                    c.Item().Text(car.ModelName).Bold().FontSize(12);
                                    c.Item().Text($"{car.Color} • {car.ModelYear}").FontSize(10);
                                    c.Item().Text($"Plate: {car.PlateNumber}").FontSize(10).Bold();
                                    c.Item().Text($"Category: {car.CategoryName}").FontSize(10);
                                }
                            });
                        });

                        col.Item().Height(20);

                        // 2. Rental Terms Table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("PICK-UP DATE");
                                header.Cell().Element(CellStyle).Text("RETURN DATE");
                                header.Cell().Element(CellStyle).Text("DURATION");
                                header.Cell().Element(CellStyle).AlignRight().Text("RATE / DAY");

                                // Removing static and using local variables is fine if not static
                                IContainer CellStyle(IContainer container)
                                {
                                    return container.Background(dark).Padding(5).BorderBottom(1).BorderColor(gold);
                                }
                            });

                            var days = (booking.Enddate.ToDateTime(TimeOnly.MinValue) - booking.Startdate.ToDateTime(TimeOnly.MinValue)).Days;
                            
                            table.Cell().Element(CellStyle).Text(booking.Startdate.ToString("dd MMM yyyy"));
                            table.Cell().Element(CellStyle).Text(booking.Enddate.ToString("dd MMM yyyy"));
                            table.Cell().Element(CellStyle).Text($"{days} Days");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{car?.PricePerDay:C}");

                            IContainer CellStyle(IContainer container)
                            {
                                return container.Padding(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            }
                        });
                        
                        // 3. Financials
                        col.Item().PaddingTop(10).AlignRight().Column(c => 
                        {
                            c.Item().Row(r => { r.RelativeItem().Text("Subtotal:"); r.RelativeItem().AlignRight().Text($"{booking.Subtotal:C}"); });
                            
                            if (promo != null)
                            {
                                c.Item().Row(r => { 
                                    r.RelativeItem().Text($"Discount ({promo.Name} {promo.DiscountPercentage}%):").FontColor(Colors.Green.Medium); 
                                    r.RelativeItem().AlignRight().Text($"-{(booking.Subtotal * promo.DiscountPercentage / 100):C}").FontColor(Colors.Green.Medium); 
                                });
                            }
                            
                            c.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Row(r => 
                            { 
                                r.RelativeItem().Text("TOTAL ESTIMATED CHARGES:").Bold(); 
                                r.RelativeItem().AlignRight().Text($"{booking.TotalPrice:C}").Bold().FontSize(14).FontColor(gold); 
                            });
                             
                             if(payment != null)
                             {
                                 c.Item().PaddingTop(5).Row(r => { 
                                     r.RelativeItem().Text("Payment Status:"); 
                                     r.RelativeItem().AlignRight().Text("PAID").Bold().FontColor(Colors.Green.Medium); 
                                 });
                             }
                        });

                        col.Item().Height(30);

                        // 4. Terms
                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(c => 
                        {
                            c.Item().Text("TERMS AND CONDITIONS").Bold().FontSize(10);
                            c.Item().Text("1. The Lessee acknowledges that the vehicle is in good operating condition.").FontSize(8);
                            c.Item().Text("2. The Lessee agrees to return the vehicle on the specified return date.").FontSize(8);
                            c.Item().Text("3. The Lessee is responsible for all traffic violations and fines incurred during the rental period.").FontSize(8);
                            c.Item().Text("4. Damage or loss of the vehicle will be charged to the Lessee according to the insurance policy.").FontSize(8);
                            c.Item().Text("5. No smoking inside the vehicle.").FontSize(8);
                        });

                        col.Item().Height(40);

                        // 5. Signatures
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Lessee Signature:").FontSize(10).FontColor(Colors.Grey.Medium);
                                c.Item().Height(30).BorderBottom(1).BorderColor(Colors.Black);
                                c.Item().Text(customer?.Name ?? "Customer").FontSize(8);
                            });
                            
                            row.ConstantItem(40);

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Company Representative:").FontSize(10).FontColor(Colors.Grey.Medium);
                                c.Item().Height(30).BorderBottom(1).BorderColor(Colors.Black);
                                c.Item().Text("Lebanon Drive RentACar").FontSize(8);
                            });
                        });
                    });
                    
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Lebanon Drive RentACar - Standard Rental Agreement - Page ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}
