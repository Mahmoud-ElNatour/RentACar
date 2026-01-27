using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentACar.Application.Managers;
using RentACar.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace RentACar.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly CustomerManager _customerManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailManager _emailManager;

        public RegisterModel(
            CustomerManager customerManager, 
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            EmailManager emailManager)
        {
            _customerManager = customerManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailManager = emailManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = default!;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = default!;

            [Required]
            [Phone]
            public string PhoneNumber { get; set; } = default!;

            [Required]
            public string Address { get; set; } = default!;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
            public string ConfirmPassword { get; set; } = default!;

            [Required]
            public IFormFile DrivingLicenseFront { get; set; } = default!;

            [Required]
            public IFormFile DrivingLicenseBack { get; set; } = default!;

            [Required]
            public IFormFile NationalIdFront { get; set; } = default!;

            [Required]
            public IFormFile NationalIdBack { get; set; } = default!;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            byte[] licenseFront = await ConvertToByteArray(Input.DrivingLicenseFront);
            byte[] licenseBack = await ConvertToByteArray(Input.DrivingLicenseBack);
            byte[] idFront = await ConvertToByteArray(Input.NationalIdFront);
            byte[] idBack = await ConvertToByteArray(Input.NationalIdBack);

            var createDto = new CustomerCreateDTO
            {
                Name = Input.FullName,
                Address = Input.Address,
                DrivingLicenseFront = licenseFront,
                DrivingLicenseBack = licenseBack,
                NationalIdfront = idFront,
                NationalIdback = idBack,
                Email = Input.Email,
                Username = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                Password = Input.Password
            };

            var result = await _customerManager.CreateCustomer(createDto);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create account. Please check your inputs.");
                return Page();
            }

            var user = await _customerManager.GetIdentityUserByEmail(Input.Email);
            if (user != null)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = Url.Content("~/") },
                    protocol: Request.Scheme);

                await _emailManager.SendConfirmationEmailAsync(Input.Email, HtmlEncoder.Default.Encode(callbackUrl), Input.FullName);

                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = Url.Content("~/") });
            }

            ModelState.AddModelError(string.Empty, "User created but could not sign in.");
            return Page();
        }

        private async Task<byte[]> ConvertToByteArray(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
