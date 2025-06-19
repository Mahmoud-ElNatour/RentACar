using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentACar.Application.Managers;
using RentACar.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace RentACar.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly CustomerManager _customerManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public RegisterModel(CustomerManager customerManager, SignInManager<IdentityUser> signInManager)
        {
            _customerManager = customerManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public IFormFile DrivingLicenseFront { get; set; }

            [Required]
            public IFormFile DrivingLicenseBack { get; set; }

            [Required]
            public IFormFile NationalIdFront { get; set; }

            [Required]
            public IFormFile NationalIdBack { get; set; }
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
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email });

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
