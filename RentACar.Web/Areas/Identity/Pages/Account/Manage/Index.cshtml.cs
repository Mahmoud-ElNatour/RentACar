﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentACar.Application.Managers;
using RentACar.Application.DTOs;

namespace RentACar.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly CustomerManager _customerManager;
        private readonly EmployeeManager _employeeManager;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            CustomerManager customerManager,
            EmployeeManager employeeManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _customerManager = customerManager;
            _employeeManager = employeeManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            CustomerDTO? customer = null;
            EmployeeDto? employee = null;

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                customer = await _customerManager.GetCustomerByUsername(userName);
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

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            CustomerDTO? customer = null;
            EmployeeDto? employee = null;

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                customer = await _customerManager.GetCustomerByUsername(user.UserName);
            }
            else if (await _userManager.IsInRoleAsync(user, "Employee") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                employee = await _employeeManager.GetEmployeeByUsername(user.UserName);
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.Email != user.Email)
            {
                var setEmail = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmail.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set email.";
                    return RedirectToPage();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (customer != null)
            {
                if (Input.Name != customer.Name)
                    await _customerManager.UpdateCustomerName(customer.UserId, Input.Name);
                if (Input.Address != customer.Address)
                    await _customerManager.UpdateCustomerAddress(customer.UserId, Input.Address);
            }
            else if (employee != null)
            {
                if (Input.Name != employee.Name)
                    await _employeeManager.UpdateEmployeeName(employee.EmployeeId.ToString(), Input.Name);
                if (Input.Address != employee.Address)
                    await _employeeManager.UpdateEmployeeAddress(employee.EmployeeId.ToString(), Input.Address);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
