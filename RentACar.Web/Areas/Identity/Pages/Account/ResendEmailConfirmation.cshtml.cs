// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace RentACar.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RentACar.Application.Managers.EmailManager _emailManager;

        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, RentACar.Application.Managers.EmailManager emailManager)
        {
            _userManager = userManager;
            _emailManager = emailManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string email = null)
        {
            if (email != null)
            {
                Input = new InputModel { Email = email };
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                TempData["ResendSuccess"] = "Verification email sent. Please check your email.";
                return RedirectToPage();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            
            await _emailManager.SendConfirmationEmailAsync(Input.Email, callbackUrl);

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            TempData["ResendSuccess"] = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
