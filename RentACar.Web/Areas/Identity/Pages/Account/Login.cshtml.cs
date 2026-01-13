// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RentACar.Application.Managers;
using RentACar.Core.Repositories;

namespace RentACar.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BlacklistManager _blacklistManager;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly AuditLogManager _auditLogManager;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<IdentityUser> userManager,
                          BlacklistManager blacklistManager,
                          ICustomerRepository customerRepository,
                          IEmployeeRepository employeeRepository,
                          AuditLogManager auditLogManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _blacklistManager = blacklistManager;
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
            _auditLogManager = auditLogManager;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            // ... (OnGetBody remains the same)
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    // ... (Blacklist and Inactive checks remain same)
                     var bl = await _blacklistManager.GetBlacklistByUserIdAsync(user.Id);
                    if (bl != null)
                    {
                        TempData["LoginError"] = "You are blacklisted.";
                        return Page();
                    }

                    var customer = await _customerRepository.GetByIdAsync(user.Id);
                    if (customer != null && !customer.Isactive)
                    {
                        TempData["LoginError"] = "Your account is inactive.";
                        return Page();
                    }

                    var employee = await _employeeRepository.GetByIdAsync(user.Id);
                    if (employee != null && !employee.IsActive)
                    {
                        TempData["LoginError"] = "Your account is inactive.";
                        return Page();
                    }

                    // Strict Check: Enforce Email Confirmation Manually
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        TempData["LoginError"] = "You must confirm your email before logging in. Please check your inbox.";
                        await _auditLogManager.LogAsync("Login", "User", user.Id, $"User {Input.Email} attempted login without confirmed email.", "Failed", Input.Email, "User");
                        return Page();
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    // Log to Audit Log
                    var roles = await _userManager.GetRolesAsync(user);
                    string roleStr = roles.Any() ? string.Join(", ", roles) : "User";
                    await _auditLogManager.LogAsync("Login", "User", user?.Id ?? "Unknown", $"User {Input.Email} logged in successfully.", "Success", Input.Email, roleStr);
                    
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                     await _auditLogManager.LogAsync("Login", "User", user?.Id ?? "Unknown", $"User {Input.Email} account locked out.", "Failed", Input.Email, "User");
                    return RedirectToPage("./Lockout");
                }
                if (result.IsNotAllowed)
                {
                   TempData["LoginError"] = "Login not allowed. Please check your account status.";
                   return Page();
                }
                
                TempData["LoginError"] = "Invalid login attempt.";
                await _auditLogManager.LogAsync("Login", "User", "Unknown", $"Invalid login attempt for {Input.Email}.", "Failed", Input.Email, "Anonymous");
                return Page();
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
