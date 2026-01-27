using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly CustomerRatingManager _ratingManager;
        private readonly CustomerManager _customerManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RatingsController(
            CustomerRatingManager ratingManager, 
            CustomerManager customerManager, 
            UserManager<IdentityUser> userManager)
        {
            _ratingManager = ratingManager;
            _customerManager = customerManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddRating([FromBody] AddRatingRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User is not logged in." });
                }

                var customer = await _customerManager.GetCustomerByAspNetUserId(user.Id);
                if (customer == null)
                {
                    return BadRequest(new { message = "Current user is not a registered customer." });
                }

                await _ratingManager.AddRatingAsync(
                    customer.UserId,
                    request.BookingId,
                    request.Stars,
                    request.Feedback
                );

                return Ok(new { message = "Rating added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Failed to save rating",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeRatings(int employeeId)
        {
            var ratings = await _ratingManager.GetRatingsByEmployeeIdAsync(employeeId);
            return Ok(ratings);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRatings(int userId)
        {
            var ratings = await _ratingManager.GetRatingsByUserIdAsync(userId);
            return Ok(ratings);
        }

        [HttpGet("summary/{employeeId}")]
        public async Task<IActionResult> GetRatingSummary(int employeeId)
        {
            var summary = await _ratingManager.GetEmployeeRatingSummaryAsync(employeeId);
            return Ok(summary);
        }
    }

    public class AddRatingRequest
    {
        public int BookingId { get; set; }
        public int Stars { get; set; }
        public string? Feedback { get; set; }
    }
}