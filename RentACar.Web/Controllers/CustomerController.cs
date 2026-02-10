using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private readonly CustomerManager _customerManager;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailManager _emailManager;

        public CustomerController(
            CustomerManager customerManager, 
            IMapper mapper, 
            ILogger<CustomerController> logger,
            UserManager<IdentityUser> userManager,
            EmailManager emailManager)
        {
            _customerManager = customerManager;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _emailManager = emailManager;
        }

        [HttpGet("~/Customer")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Customer/Index.cshtml");
        }

        [HttpGet("~/Customer/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Customer/_CustomerFormPartial.cshtml", new CustomerDTO { Isactive = true });
        }

        [HttpGet("~/Customer/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_CustomerFormPartial.cshtml", customer);
        }

        [HttpGet("~/Customer/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_DeleteCustomerPartial.cshtml", customer);
        }

        [HttpGet("~/Customer/Documents/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DocumentsForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_CustomerDocumentsPartial.cshtml", customer);
        }

        [HttpGet("~/Customer/DetailsPartial/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DetailsPartial(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_CustomerDetailsPartial.cshtml", customer);
        }

        [HttpGet("~/Customer/SummaryPartial/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SummaryPartial(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_CustomerSummaryPartial.cshtml", customer);
        }


        [HttpGet("GetFilteredCustomers")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<PagedResultDto<CustomerListDto>>> GetFilteredCustomers(
            [FromQuery] string? search, 
            [FromQuery] bool? verified, 
            [FromQuery] bool? active,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortColumn = "Name",
            [FromQuery] string? sortDirection = "asc")
        {
            try
            {
                var result = await _customerManager.GetCustomersPagedAsync(search, verified, active, page, pageSize, sortColumn, sortDirection);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load customers paged");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("Search")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<object>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Ok(new List<object>());

            var customers = await _customerManager.GetCustomersPagedAsync(query, null, null, 1, 10, "Name", "asc");
            var result = customers.Items.Select(c => new 
            {
                id = c.UserId,
                text = $"{c.Name} (ID: {c.UserId}) - {c.PhoneNumber}",
                email = c.Email,
                name = c.Name
            });
            return Ok(result);
        }


        [HttpGet("Document/{id}/{type}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDocument(int id, string type)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();

            byte[]? imageBytes = type switch
            {
                "license-front" => customer.DrivingLicenseFront,
                "license-back" => customer.DrivingLicenseBack,
                "id-front" => customer.NationalIdfront,
                "id-back" => customer.NationalIdback,
                _ => null
            };

            if (imageBytes == null || imageBytes.Length == 0)
                return NotFound();

            return File(imageBytes, "image/jpeg");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> Get(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> Create([FromBody] CustomerCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating customer");

            try
            {
                var created = await _customerManager.CreateCustomer(dto);
                return CreatedAtAction(nameof(Get), new { id = created!.UserId }, created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create customer: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDTO dto)
        {
            if (id != dto.UserId) return BadRequest();
            _logger.LogInformation("Updating customer {Id}", id);
            try
            {
                await _customerManager.UpdateCustomer(dto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update customer {Id}: {Message}", id, ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting customer {Id}", id);

            try
            {
                await _customerManager.DeleteCustomer(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Customer {Id} could not be deleted: {Message}", id, ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint prevented deleting customer {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Unable to delete customer because related records exist. Remove the related data before deleting the customer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting customer {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while deleting the customer. Please try again later.");
            }
        }

        [HttpGet("~/Customer/ResetPassword/{id}")]

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ResetPasswordForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_ResetPasswordPartial.cshtml", customer);
        }

        [HttpPost("{id}/reset-password")]

        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest? request)
        {
            var success = await _customerManager.ResetPassword(id, request?.NewPassword);
            if (!success) return NotFound();
            return Ok(new { message = "Password reset successfully and email sent." });
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Verify(int id)
        {
            await _customerManager.UpdateVerificationStatus(id, true);
            return Ok(new { message = "Customer verified successfully." });
        }

        [HttpPost("{id}/resend-verification")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> ResendVerification(int id)
        {
            var s = await _customerManager.SendReminderToCustomerAsync(id);
            if (!s) return BadRequest("Could not send reminder. Customer might already be verified or email failed.");
            return Ok(new { message = "Document verification reminder sent." });
        }
        
        public class ResetPasswordRequest
        {
            public string? NewPassword { get; set; }
        }

        [HttpPost("{id}/resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound("Customer not found");

            var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
            if (user == null) return NotFound("Associated user account not found");

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            // Construct the callback URL manually since we are in API controller context
            // and Url.Page helper usually generates relative URLs unless protocol is specified.
            // We'll point to the Identity Area's ConfirmEmail page.
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailManager.SendConfirmationEmailAsync(user.Email ?? string.Empty, callbackUrl, customer.Name);

            return Ok(new { message = "Confirmation email sent successfully." });
        }
    }
}
