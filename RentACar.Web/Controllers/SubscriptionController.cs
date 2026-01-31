using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly RentACarDbContext _context;
        private readonly EmailManager _emailManager;

        public SubscriptionController(RentACarDbContext context, EmailManager emailManager)
        {
            _context = context;
            _emailManager = emailManager;
        }

        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
            {
                return BadRequest(new { success = false, message = "Email is required." });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { success = false, message = "Please enter a valid email address." });
            }

            var email = request.Email.Trim().ToLowerInvariant();

            try
            {
                // 1. Find or Create "Newsletter Subscribers" List
                var list = await _context.DistributionLists
                    .FirstOrDefaultAsync(l => l.Name == "Newsletter Subscribers");

                if (list == null)
                {
                    list = new DistributionList
                    {
                        Name = "Newsletter Subscribers",
                        Description = "Auto-generated list for footer subscriptions",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.DistributionLists.Add(list);
                    await _context.SaveChangesAsync();
                }

                // 2. Check overlap
                var existingMember = await _context.DistributionListMembers
                    .FirstOrDefaultAsync(m => m.DistributionListId == list.Id && m.Email == email);

                if (existingMember != null)
                {
                    if (!existingMember.IsActive)
                    {
                        existingMember.IsActive = true;
                        existingMember.AddedAt = DateTime.UtcNow; // update timestamp
                        await _context.SaveChangesAsync();
                        
                        // Send welcome back email
                         await _emailManager.SendEmailSafeAsync(
                            email, 
                            "Welcome back to our Newsletter!", 
                            $"<h2>Welcome Back!</h2><p>You have successfully reactivated your subscription to the Rent A Car newsletter.</p>", 
                            "Newsletter Reactivation"
                        );
                        
                        return Ok(new { success = true, message = "Welcome back! Subscription reactivated." });
                    }
                    
                    return Ok(new { success = true, message = "You are already subscribed!" });
                }

                // 3. Add new member
                var newMember = new DistributionListMember
                {
                    DistributionListId = list.Id,
                    Email = email,
                    Label = request.Name ?? "Subscriber",
                    MemberType = "Subscriber",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow
                };

                _context.DistributionListMembers.Add(newMember);
                await _context.SaveChangesAsync();

                // 4. Send Welcome Email
                bool emailSent = await _emailManager.SendEmailSafeAsync(
                    email, 
                    "Welcome to our Newsletter!", 
                    $"<h2>Thanks for subscribing!</h2><p>You're now on the list to receive our latest news and exclusive offers.</p>", 
                    "Newsletter Welcome"
                );

            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, new { success = false, message = "An error occurred while subscribing." });
            }

            return Ok(new { success = true, message = "Successfully subscribed!" });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email.Trim()) return false;

                // Strict check: Require a dot in the domain part (after the @)
                var idx = email.LastIndexOf('@');
                if (idx > 0 && email.IndexOf('.', idx) > idx + 1)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public class SubscribeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}
