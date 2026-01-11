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

        public List<CreditCardDto> CreditCards { get; set; } = new();

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

        public class InputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }
        }

        [BindProperty]
        public CreditCardInputModel NewCreditCard { get; set; }

        public class CreditCardInputModel
        {
            [Required]
            [Display(Name = "Card Number")]
            // ✅ We will store as digits only; UI may send spaces (we normalize before save)
            [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits.")]
            public string CardNumber { get; set; }

            [Required]
            public string CardHolderName { get; set; }

            [Required]
            [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Invalid Expiry Format (MM/YY)")]
            public string ExpiryDate { get; set; }

            [Required]
            [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Invalid CVV")]
            public string Cvv { get; set; }
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

                    var cards = await _dbContext.CustomerCreditCards
                        .Where(c => c.UserId == customer.UserId)
                        .Include(c => c.CreditCard)
                        .Select(c => c.CreditCard)
                        .ToListAsync();

                    CreditCards = cards.Select(c => new CreditCardDto
                    {
                        CreditCardId = c.CreditCardId,
                        CardHolderName = c.CardHolderName,
                        CardNumber = c.CardNumber,
                        ExpiryDate = c.ExpiryDate,
                        Cvv = c.Cvv
                    }).ToList();
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
            if (await _userManager.IsInRoleAsync(user, "Customer"))
                customer = await _customerManager.GetCustomerByUsername(user.UserName);

            if (customer != null)
            {
                if (Input.Name != customer.Name) await _customerManager.UpdateCustomerName(customer.UserId, Input.Name);
                if (Input.Address != customer.Address) await _customerManager.UpdateCustomerAddress(customer.UserId, Input.Address);
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

        public async Task<IActionResult> OnPostAddCreditCardAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (NewCreditCard == null)
            {
                StatusMessage = "Error: Invalid card data.";
                return RedirectToPage();
            }

            // ✅ Normalize card number: remove spaces/dashes
            NewCreditCard.CardNumber = (NewCreditCard.CardNumber ?? "").Replace(" ", "").Replace("-", "");

            // Trigger DataAnnotations validation after normalization
            TryValidateModel(NewCreditCard, nameof(NewCreditCard));
            if (!ModelState.IsValid)
            {
                StatusMessage = "Error: Please fix the card form errors.";
                return RedirectToPage();
            }

            if (!DateTime.TryParseExact(NewCreditCard.ExpiryDate, "MM/yy", null,
                    System.Globalization.DateTimeStyles.None, out var expiryParsed))
            {
                StatusMessage = "Error: Invalid Expiry Date format.";
                return RedirectToPage();
            }

            // Store as DateOnly: first day of month
            var expiry = new DateOnly(expiryParsed.Year, expiryParsed.Month, 1);

            var customerDto = await _customerManager.GetCustomerByUsername(user.UserName);
            if (customerDto == null) return RedirectToPage();

            var newCard = new CreditCard
            {
                CardNumber = NewCreditCard.CardNumber,
                CardHolderName = NewCreditCard.CardHolderName,
                ExpiryDate = expiry,
                Cvv = NewCreditCard.Cvv
            };

            _dbContext.CreditCards.Add(newCard);
            await _dbContext.SaveChangesAsync();

            _dbContext.CustomerCreditCards.Add(new CustomerCreditCard
            {
                UserId = customerDto.UserId,
                CreditCardId = newCard.CreditCardId
            });

            await _dbContext.SaveChangesAsync();

            StatusMessage = "Credit Card added successfully";
            return RedirectToPage();
        }

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

        public async Task<IActionResult> OnPostDeleteCreditCardAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _dbContext.Customers
                .Include(c => c.CustomerCreditCards)
                .ThenInclude(cc => cc.CreditCard)
                .FirstOrDefaultAsync(c => c.aspNetUserId == user.Id);

            if (customer == null) return NotFound();

            var creditCardLink = customer.CustomerCreditCards.FirstOrDefault(cc => cc.CreditCardId == id);
            if (creditCardLink == null)
            {
                StatusMessage = "Error: Card not found.";
                return RedirectToPage();
            }

            _dbContext.Set<CustomerCreditCard>().Remove(creditCardLink);

            if (creditCardLink.CreditCard != null)
                _dbContext.CreditCards.Remove(creditCardLink.CreditCard);

            await _dbContext.SaveChangesAsync();

            StatusMessage = "Credit card deleted successfully";
            return RedirectToPage();
        }

        private async Task<byte[]> GetBytes(IFormFile file)
        {
            using var ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
