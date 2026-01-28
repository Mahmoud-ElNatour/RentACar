// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentACar.Application.Managers;

namespace RentACar.Web.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly EmailManager _emailManager;

        public LoginWith2faModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginWith2faModel> logger,
            EmailManager emailManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailManager = emailManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }



        public string MfaProvider { get; set; }
        public bool ShowMethodSwitcher { get; set; }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null, string mfaProvider = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            ShowMethodSwitcher = providers.Count > 1;

            // Determine provider: Requested -> Authenticator (if available) -> Email (fallback)
            if (!string.IsNullOrEmpty(mfaProvider) && providers.Contains(mfaProvider))
            {
                MfaProvider = mfaProvider;
            }
            else if (providers.Contains("Authenticator")) 
            {
                MfaProvider = "Authenticator";
            }
            else if (providers.Contains("Email"))
            {
                MfaProvider = "Email";
            }
            else 
            {
                MfaProvider = providers.FirstOrDefault();
            }

            // If Email, send generic code
            if (MfaProvider == "Email")
            {
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                await _emailManager.SendOtpEmailAsync(user.Email, code, user.UserName ?? "User");
                // Note: We don't cache this OTP manually because Identity's `GenerateTwoFactorTokenAsync` handles the token generation/validation lifecycle for 2FA login.
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null, string mfaProvider = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) throw new InvalidOperationException($"Unable to load two-factor authentication user.");

            // Normalize code
            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            
            // Re-determine provider to ensure we verify against the correct one
            // We trust the hidden input or query param, but server side re-verification of "Valid" providers is handled by `TwoFactorSignInAsync` internally? No, we must pass the provider name.
            // But `TwoFactorAuthenticatorSignInAsync` is ONLY for TOTP. `TwoFactorSignInAsync` creates the cookie.
            // Actually `TwoFactorSignInAsync` takes `provider, code, rememberMe, rememberMachine`. This works for BOTH Email and Authenticator if we pass "Authenticator" as provider name.

            // Ensure provider is set
            if (string.IsNullOrEmpty(mfaProvider))
            {
                // Fallback logic identical to OnGet to guess what the user likely used
                var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
                if (providers.Contains("Authenticator")) mfaProvider = "Authenticator";
                else if (providers.Contains("Email")) mfaProvider = "Email";
                else mfaProvider = providers.FirstOrDefault();
            }
            
            MfaProvider = mfaProvider; // For redisplay if failed

            // Execute Sign In
            var result = await _signInManager.TwoFactorSignInAsync(mfaProvider, authenticatorCode, rememberMe, Input.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return Page();
            }
        }
    }
}
