using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Managers;
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            BookingManager bookingManager,
            UserManager<IdentityUser> userManager,
            ILogger<BookingController> logger)
        {
            _bookingManager = bookingManager;
            _userManager = userManager;
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
        public IActionResult Add()
        {
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> Get()
        {
            var bookings = await _bookingManager.GetAllBookingsAsync();
            return Ok(bookings);
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
        public async Task<IActionResult> Update(int id, [FromBody] BookingDto dto)
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
    }
}
