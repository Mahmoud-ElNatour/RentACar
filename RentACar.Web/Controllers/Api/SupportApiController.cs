using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs.Support;
using RentACar.Application.Managers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SupportApiController : ControllerBase
    {
        private readonly SupportManager _supportManager;

        public SupportApiController(SupportManager supportManager)
        {
            _supportManager = supportManager;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _supportManager.GetCustomerConversationsPagedAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("my/{id}")]
        public async Task<IActionResult> GetMyConversationDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _supportManager.GetConversationDetailsForCustomerAsync(id, userId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateSupportConversationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var id = await _supportManager.CreateConversationAsync(userId, dto);
            return Ok(new { id });
        }

        [HttpPost("{id}/message")]
        public async Task<IActionResult> SendMessage(int id, [FromBody] SendSupportMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (id != dto.ConversationId) return BadRequest();

            var success = await _supportManager.SendMessageAsCustomerAsync(userId, dto);
            if (!success) return BadRequest("Could not send message. The conversation might be closed or doesn't belong to you.");

            return Ok();
        }

        // --- Employee Endpoints ---

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? category = null, [FromQuery] string? searchQuery = null, [FromQuery] string? assignedEmployeeId = null)
        {
            var result = await _supportManager.GetAllConversationsPagedAsync(page, pageSize, status, category, searchQuery, assignedEmployeeId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("inbox/{id}")]
        public async Task<IActionResult> GetInboxDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _supportManager.GetConversationDetailsForEmployeeAsync(id, userId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("inbox/{id}/message")]
        public async Task<IActionResult> SendEmployeeMessage(int id, [FromBody] SendSupportMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (id != dto.ConversationId) return BadRequest();

            var success = await _supportManager.SendMessageAsEmployeeAsync(userId, dto);
            if (!success) return BadRequest("Could not send message.");

            return Ok();
        }

        [HttpPost("inbox/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSupportConversationStatusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (id != dto.ConversationId) return BadRequest();

            // Permission Check
            if (User.IsInRole("Customer"))
            {
                // Verify ownership
                var conversation = await _supportManager.GetConversationDetailsForCustomerAsync(id, userId);
                if (conversation == null) return Forbid();
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                return Forbid();
            }

            await _supportManager.UpdateStatusAsync(userId, id, dto.NewStatus);
            return Ok();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("inbox/{id}/assign")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignSupportConversationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (id != dto.ConversationId) return BadRequest();
            
            await _supportManager.ReassignAsync(userId, id, dto.AssignedEmployeeId, dto.Note ?? "API Assignment");
            return Ok();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("inbox/{id}/internal-note")]
        public async Task<IActionResult> AddInternalNote(int id, [FromBody] string messageText)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _supportManager.AddInternalNoteAsync(userId, id, messageText);
            return Ok();
        }
    }
}
