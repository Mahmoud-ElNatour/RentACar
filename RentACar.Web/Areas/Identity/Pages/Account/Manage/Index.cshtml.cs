// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly CustomerManager _customerManager;
        private readonly EmployeeManager _employeeManager;
        private readonly EmailManager _emailManager;
        private readonly IMemoryCache _memoryCache;
        private readonly RentACarDbContext _dbContext;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            CustomerManager customerManager,
            EmployeeManager employeeManager,
            EmailManager emailManager,
            IMemoryCache memoryCache,
            RentACarDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _customerManager = customerManager;
            _employeeManager = employeeManager;
            _emailManager = emailManager;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool IsVerified { get; set; }



        public string DrivingLicenseFrontSrc { get; set; }
        public string DrivingLicenseBackSrc { get; set; }
        public string NationalIdFrontSrc { get; set; }
        public string NationalIdBackSrc { get; set; }
        public int CompletedDocsCount { get; set; }

        public bool IsOtpSent { get; set; }
        public bool IsOtpVerified { get; set; }

        [BindProperty]
        public string OTP { get; set; }

        [BindProperty, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }

        // --- MFA State ---
        public bool IsMfaUnlocked { get; set; }
        public bool HasAuthenticator { get; set; }
        public bool Is2faEnabled { get; set; }
        public bool IsEmail2faEnabled { get; set; }
        public int RecoveryCodesLeft { get; set; }

        [BindProperty]
        public string MfaOtp { get; set; }

        public class InputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, Phone]
            public string PhoneNumber { get; set; }
        }



        [BindProperty] public IFormFile UploadNationalIdFront { get; set; }
        [BindProperty] public IFormFile UploadNationalIdBack { get; set; }
        [BindProperty] public IFormFile UploadDrivingLicenseFront { get; set; }
        [BindProperty] public IFormFile UploadDrivingLicenseBack { get; set; }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            CustomerDTO customer = null;
            EmployeeDto employee = null;

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                customer = await _customerManager.GetCustomerByUsername(userName);
                if (customer != null)
                {
                    IsVerified = customer.IsVerified;

                    DrivingLicenseFrontSrc = ToBase64(customer.DrivingLicenseFront);
                    DrivingLicenseBackSrc = ToBase64(customer.DrivingLicenseBack);
                    NationalIdFrontSrc = ToBase64(customer.NationalIdfront);
                    NationalIdBackSrc = ToBase64(customer.NationalIdback);

                    CompletedDocsCount = 0;
                    if (customer.DrivingLicenseFront != null) CompletedDocsCount++;
                    if (customer.DrivingLicenseBack != null) CompletedDocsCount++;
                    if (customer.NationalIdfront != null) CompletedDocsCount++;
                    if (customer.NationalIdback != null) CompletedDocsCount++;
                }
            }
            else if (await _userManager.IsInRoleAsync(user, "Employee") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                employee = await _employeeManager.GetEmployeeByUsername(userName);
            }

            Username = userName;

            Input = new InputModel
            {
                Name = customer?.Name ?? employee?.Name,
                Address = customer?.Address ?? employee?.Address,
                Email = email,
                PhoneNumber = phoneNumber
            };

            // --- Load MFA Status ---
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsEmail2faEnabled = Is2faEnabled && !HasAuthenticator; // Simple logic: if 2FA is on but no auth app, it must be email (or we default to it)
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            // Check if we just unlocked MFA in this session
            if (TempData["IsMfaUnlocked"] as bool? == true) IsMfaUnlocked = true;
            if (IsMfaUnlocked) TempData.Keep("IsMfaUnlocked");
        }

        private string ToBase64(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (TempData["IsOtpSent"] as bool? == true) IsOtpSent = true;
            if (TempData["IsOtpVerified"] as bool? == true) IsOtpVerified = true;

            if (IsOtpSent) TempData.Keep("IsOtpSent");
            if (IsOtpVerified) TempData.Keep("IsOtpVerified");
            if (TempData["VerifiedOTP"] != null) TempData.Keep("VerifiedOTP");
            if (TempData["RecoveryCodes"] != null) TempData.Keep("RecoveryCodes");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            CustomerDTO customer = null;
            EmployeeDto employee = null;

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                customer = await _customerManager.GetCustomerByAspNetUserId(user.Id);
            }
            else if (await _userManager.IsInRoleAsync(user, "Employee") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                employee = await _employeeManager.GetEmployeeByAspNetUserId(user.Id);
            }

            if (customer != null)
            {
                // Update properties on DTO
                customer.Name = Input.Name;
                customer.Address = Input.Address;
                // customer.IsVerified & Isactive remain unchanged here or pulled from source if DTO doesn't have them in input
                
                // Call full update method (Fixes: "not update on the db")
                await _customerManager.UpdateCustomer(customer);
            }
            else if (employee != null)
            {
                employee.Name = Input.Name;
                employee.Address = Input.Address;
                await _employeeManager.UpdateEmployee(employee);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profile updated successfully";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendOtpAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"OTP_{user.Id}";

            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));
            await _emailManager.SendOtpEmailAsync(user.Email, otp, user.UserName);

            StatusMessage = "OTP sent to your email";
            TempData["IsOtpSent"] = true;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVerifyOtpAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cacheKey = $"OTP_{user.Id}";
            if (!_memoryCache.TryGetValue(cacheKey, out string storedOtp) || storedOtp != OTP)
            {
                StatusMessage = "Error: Invalid or expired OTP";
                TempData["IsOtpSent"] = true;
                return RedirectToPage();
            }

            StatusMessage = "OTP Verified";
            TempData["IsOtpSent"] = true;
            TempData["IsOtpVerified"] = true;
            TempData["VerifiedOTP"] = OTP;
            return RedirectToPage();
        }

        // ✅ Handler name is ResetPasswordWithOtp (NOT ResetPasswordWithOtpAsync)
        public async Task<IActionResult> OnPostResetPasswordWithOtpAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var verifiedOtp = TempData["VerifiedOTP"] as string;

            if (string.IsNullOrEmpty(verifiedOtp) && string.IsNullOrEmpty(OTP))
            {
                StatusMessage = "Error: Session expired or invalid OTP";
                return RedirectToPage();
            }

            var cacheKey = $"OTP_{user.Id}";
            if (!_memoryCache.TryGetValue(cacheKey, out string storedOtp) || storedOtp != (verifiedOtp ?? OTP))
            {
                StatusMessage = "Error: OTP expired";
                return RedirectToPage();
            }

            if (string.IsNullOrEmpty(NewPassword))
            {
                StatusMessage = "Error: Password cannot be empty";
                TempData["IsOtpSent"] = true;
                TempData["IsOtpVerified"] = true;
                TempData["VerifiedOTP"] = verifiedOtp;
                return RedirectToPage();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);

            if (result.Succeeded)
            {
                StatusMessage = "Password changed successfully";
                _memoryCache.Remove(cacheKey);

                TempData.Remove("IsOtpSent");
                TempData.Remove("IsOtpVerified");
                TempData.Remove("VerifiedOTP");

                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                StatusMessage = "Error: " + string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["IsOtpSent"] = true;
                TempData["IsOtpVerified"] = true;
                TempData["VerifiedOTP"] = verifiedOtp;
            }

            return RedirectToPage();
        }

        // Credit card handlers removed - payments handled via Stripe

        public async Task<IActionResult> OnPostUploadDocumentsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.aspNetUserId == user.Id);
            if (customer == null) return NotFound();

            bool anyUploaded = false;

            if (UploadNationalIdFront != null) { customer.NationalIdfront = await GetBytes(UploadNationalIdFront); anyUploaded = true; }
            if (UploadNationalIdBack != null) { customer.NationalIdback = await GetBytes(UploadNationalIdBack); anyUploaded = true; }
            if (UploadDrivingLicenseFront != null) { customer.DrivingLicenseFront = await GetBytes(UploadDrivingLicenseFront); anyUploaded = true; }
            if (UploadDrivingLicenseBack != null) { customer.DrivingLicenseBack = await GetBytes(UploadDrivingLicenseBack); anyUploaded = true; }

            if (!anyUploaded)
            {
                StatusMessage = "Error: No files selected.";
                return RedirectToPage();
            }

            _dbContext.Customers.Update(customer);
            await _dbContext.SaveChangesAsync();

            StatusMessage = "Documents uploaded successfully";
            return RedirectToPage();
        }

        private async Task<byte[]> GetBytes(IFormFile file)
        {
            using var ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        // --- MFA Handlers ---

        public async Task<IActionResult> OnPostSendMfaOtpAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"MFA_OTP_{user.Id}";

            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));
            await _emailManager.SendOtpEmailAsync(user.Email, otp, user.UserName);

            StatusMessage = "Verification code sent to email.";
            TempData["IsMfaOtpSent"] = true; // Flag to show OTP input
            return RedirectToPage(); // Helper logic in frontend will open this tab
        }

        public async Task<IActionResult> OnPostVerifyMfaOtpAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cacheKey = $"MFA_OTP_{user.Id}";
            if (!_memoryCache.TryGetValue(cacheKey, out string storedOtp) || storedOtp != MfaOtp)
            {
                StatusMessage = "Error: Invalid or expired code.";
                TempData["IsMfaOtpSent"] = true;
                return RedirectToPage();
            }

            // Success -> Unlock MFA settings
            StatusMessage = "Identity Verified. You can now manage 2FA.";
            TempData["IsMfaUnlocked"] = true;
            _memoryCache.Remove(cacheKey);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisable2faAsync()
        {
            // Requires MFA Unlock
            if (TempData["IsMfaUnlocked"] as bool? != true) return RedirectToPage();
            TempData.Keep("IsMfaUnlocked");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                StatusMessage = "Error: Failed to disable 2FA.";
                return RedirectToPage();
            }

            // Also reset authenticator key to be clean? No, kept in case they re-enable.
            StatusMessage = "Two-factor authentication has been disabled.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEnableEmail2faAsync()
        {
            // Requires MFA Unlock
            if (TempData["IsMfaUnlocked"] as bool? != true) return RedirectToPage();
            TempData.Keep("IsMfaUnlocked");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Enabling "Email 2FA" just means enabling 2FA contentiously.
            // Since we have DefaultTokenProviders, Email is utilized as a fallback or primary if no Authenticator.
            // The goal is: SetTwoFactorEnabledAsync(user, true).
            
            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
             if (!result.Succeeded)
            {
                StatusMessage = "Error: Failed to enable 2FA.";
                return RedirectToPage();
            }

            StatusMessage = "Email Two-Factor Authentication enabled.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostGenerateRecoveryCodesAsync()
        {
            // Requires MFA Unlock or just be verified?
            // Since this grants permanent access, it should require strict 2FA check or "IsMfaUnlocked" state.
            if (TempData["IsMfaUnlocked"] as bool? != true) return RedirectToPage();
            TempData.Keep("IsMfaUnlocked");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                StatusMessage = "Error: You must enable 2FA before generating recovery codes.";
                return RedirectToPage();
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            
            // We want to show these codes to the user.
            // We can flash them to TempData or Redirect to a "ShowRecoveryCodes" page.
            // Given the user wants it in the settings, we should probably set a property or TempData to render them in the view.
            TempData["RecoveryCodes"] = recoveryCodes.ToArray();
            
            // Send codes via email as requested
            await _emailManager.SendRecoveryCodesEmailAsync(user.Email, recoveryCodes, user.UserName);
            
            StatusMessage = "New recovery codes generated and sent to your email.";
            
            return RedirectToPage();
        }
    }
}
