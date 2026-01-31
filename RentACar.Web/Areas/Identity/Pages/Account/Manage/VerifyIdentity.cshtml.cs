using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Areas.Identity.Pages.Account.Manage
{
    public class VerifyIdentityModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CustomerManager _customerManager;

        public VerifyIdentityModel(
            UserManager<IdentityUser> userManager,
            CustomerManager customerManager)
        {
            _userManager = userManager;
            _customerManager = customerManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Current Address")]
            public string Address { get; set; }

            [Required]
            [Display(Name = "Driving License (Front)")]
            public IFormFile DrivingLicenseFront { get; set; }

            [Required]
            [Display(Name = "Driving License (Back)")]
            public IFormFile DrivingLicenseBack { get; set; }

            [Required]
            [Display(Name = "National ID (Front)")]
            public IFormFile NationalIdFront { get; set; }

            [Required]
            [Display(Name = "National ID (Back)")]
            public IFormFile NationalIdBack { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Optional: Pre-fill address if it exists? 
            // Since we moved address to this stage, it's likely empty initially.
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var customer = await _customerManager.GetCustomerByAspNetUserId(user.Id);
            if (customer == null)
            {
                return NotFound("Customer profile not found.");
            }

            // Convert Files
            byte[] licenseFront = await ConvertToByteArray(Input.DrivingLicenseFront);
            byte[] licenseBack = await ConvertToByteArray(Input.DrivingLicenseBack);
            byte[] idFront = await ConvertToByteArray(Input.NationalIdFront);
            byte[] idBack = await ConvertToByteArray(Input.NationalIdBack);

            // Update Documents
            var docsDto = new CustomerDocumentsDto
            {
                DrivingLicenseFront = licenseFront,
                DrivingLicenseBack = licenseBack,
                NationalIdfront = idFront,
                NationalIdback = idBack
            };
            
            await _customerManager.UpdateCustomerDocuments(customer.UserId, docsDto);
            
            // Update Address
            await _customerManager.UpdateCustomerAddress(customer.UserId, Input.Address);

            StatusMessage = "Your documents have been submitted for verification.";
            return RedirectToPage("/Account/Login"); // User requested Redirect to Login
        }
        
        public IActionResult OnPostSkip()
        {
             return RedirectToPage("/Account/Login"); // User requested Redirect to Login
        }

        private async Task<byte[]> ConvertToByteArray(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
