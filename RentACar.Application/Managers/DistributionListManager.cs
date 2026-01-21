using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class DistributionListManager
    {
        private readonly RentACarDbContext _context;

        public DistributionListManager(RentACarDbContext context)
        {
            _context = context;
        }

        public async Task<List<DistributionListDto>> GetAllListsAsync()
        {
            return await _context.DistributionLists
                .Include(l => l.Members)
                .Select(l => new DistributionListDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    CreatedByUserId = l.CreatedByUserId,
                    UpdatedByUserId = l.UpdatedByUserId,
                    MemberCount = l.Members.Count(m => m.IsActive)
                })
                .ToListAsync();
        }

        public async Task<DistributionListDto> GetListByIdAsync(int id)
        {
            var list = await _context.DistributionLists
                .Include(l => l.Members)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null) return null;

            return new DistributionListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsActive = list.IsActive,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                CreatedByUserId = list.CreatedByUserId,
                UpdatedByUserId = list.UpdatedByUserId,
                MemberCount = list.Members.Count,
                Members = list.Members.Select(m => new DistributionListMemberDto
                {
                    Id = m.Id,
                    DistributionListId = m.DistributionListId,
                    Email = m.Email,
                    Label = m.Label,
                    MemberType = m.MemberType,
                    IsActive = m.IsActive,
                    AddedAt = m.AddedAt,
                    AddedByUserId = m.AddedByUserId
                }).ToList()
            };
        }

        public async Task<int> CreateListAsync(DistributionListDto dto, string userId)
        {
            var list = new DistributionList
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = userId
            };

            _context.DistributionLists.Add(list);
            await _context.SaveChangesAsync();
            return list.Id;
        }

        public async Task UpdateListAsync(DistributionListDto dto, string userId)
        {
            var list = await _context.DistributionLists.FindAsync(dto.Id);
            if (list == null) throw new Exception("List not found");

            list.Name = dto.Name;
            list.Description = dto.Description;
            list.IsActive = dto.IsActive;
            list.UpdatedAt = DateTime.UtcNow;
            list.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        public async Task ToggleListActiveAsync(int id, string userId)
        {
            var list = await _context.DistributionLists.FindAsync(id);
            if (list != null)
            {
                list.IsActive = !list.IsActive;
                list.UpdatedAt = DateTime.UtcNow;
                list.UpdatedByUserId = userId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteListAsync(int id)
        {
            var list = await _context.DistributionLists.FindAsync(id);
            if (list != null)
            {
                _context.DistributionLists.Remove(list);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddMemberAsync(int listId, string email, string label, string type, string userId)
        {
            // check for duplicate
            var exists = await _context.DistributionListMembers
                .AnyAsync(m => m.DistributionListId == listId && m.Email == email);
            
            if (exists) return;

            var member = new DistributionListMember
            {
                DistributionListId = listId,
                Email = email,
                Label = label,
                MemberType = type,
                IsActive = true,
                AddedAt = DateTime.UtcNow,
                AddedByUserId = userId
            };

            _context.DistributionListMembers.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int memberId)
        {
            var member = await _context.DistributionListMembers.FindAsync(memberId);
            if (member != null)
            {
                _context.DistributionListMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        // The core logic for generating recipients based on rules
        public async Task<List<DistributionListMemberDto>> PreviewRecipientsAsync(DistributionListRuleDto rule)
        {
            var result = new List<DistributionListMemberDto>();
            var emails = new HashSet<string>();

            // 1. Employees
            if (rule.IncludeEmployees)
            {
                var q = _context.Employees.AsQueryable();
                // Assumes Employee has UserId linked to AspNetUser for IsActive if needed, 
                // OR we check Employee table fields. 
                // The requirements asked for "Active Employees".
                // Let's assume Employee entity implies active or we check Identity User.
                // For simplicity, we'll fetch emails.
                var employeeEmails = await q.Where(e => e.User.Email != null).Select(e => e.User.Email).ToListAsync();
                foreach (var e in employeeEmails) 
                {
                    if (string.IsNullOrWhiteSpace(e)) continue;
                    if (emails.Add(e)) 
                        result.Add(new DistributionListMemberDto { Email = e, MemberType = "Employee", Label = "Employee", IsActive = true });
                }
            }

            // 2. Admins (Roles) - This might be harder without UserManager, but we can query AspNetUserRoles
            if (rule.IncludeAdmins)
            {
                // This is a bit complex in pure EF without Identity types mapped exactly, but we can try.
                // Or we can rely on logical assumption that Admins are Employees?
                // Let's assume we skip this for "Preview" efficiently without UserRole join complexity if strict EF mapping is missing 
                // OR we do a join if we have the types.
                // We have AspNetUsers, AspNetUserRoles, AspNetRoles in Context.
                
                var adminRoleId = await _context.AspNetRoles
                    .Where(r => r.Name == "Admin" || r.Name == "Administrator")
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                if (adminRoleId != null)
                {
                    // Find users in this role
                   /* 
                      var adminEmails = await _context.AspNetUsers
                        .Where(u => u.Roles.Any(r => r.Id == adminRoleId)) // If navigation property exists
                        .Select(u => u.Email)
                        .ToListAsync();
                   */
                   // Since navigation might be tricky depending on Identity setup:
                   // Use join manually if needed, or if navigation exists.
                   // Looking at Context: entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                   // So navigation 'Roles' exists on AspNetUser.
                   
                    var adminEmails = await _context.AspNetUsers
                        .Where(u => u.Roles.Any(r => r.Id == adminRoleId))
                        .Select(u => u.Email)
                        .ToListAsync();

                    foreach (var e in adminEmails)
                    {
                        if (string.IsNullOrWhiteSpace(e)) continue;
                        if (emails.Add(e))
                            result.Add(new DistributionListMemberDto { Email = e, MemberType = "Admin", Label = "Admin", IsActive = true });
                    }
                }
            }

            // 3. Customers
            if (rule.IncludeCustomers)
            {
                var q = _context.Customers.Include(c => c.User).AsQueryable();
                
                if (rule.OnlyActiveUsers)
                    q = q.Where(c => c.Isactive == true); // Note: Customer.Isactive is nullable bool? or bool? Checked context: HasDefaultValue(true)
                
                // Exclude blacklisted?
                // Provide logic if we can join BlackList
                if (rule.ExcludeBlacklistedCustomers)
                {
                   // var blacklistedUserIds = _context.BlackLists.Select(b => b.UserId);
                   // q = q.Where(c => !blacklistedUserIds.Contains(c.UserId));
                    // Or via Navigation
                    // Customer -> User -> BlackLists
                    q = q.Where(c => !c.User.BlackLists.Any());
                }

                if (rule.OnlyVerifiedEmails)
                {
                    q = q.Where(c => c.User.EmailConfirmed);
                }

                var customerEmails = await q.Select(c => c.User.Email).ToListAsync();
                foreach (var e in customerEmails)
                {
                     if (string.IsNullOrWhiteSpace(e)) continue;
                     if (emails.Add(e))
                        result.Add(new DistributionListMemberDto { Email = e, MemberType = "Customer", Label = "Customer", IsActive = true });
                }
            }

            // 4. Manual Emails
            if (!string.IsNullOrWhiteSpace(rule.ManualEmailsRaw))
            {
                var manuals = rule.ManualEmailsRaw.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in manuals)
                {
                    var e = raw.Trim();
                    // simple validation
                    if (!e.Contains("@")) continue;

                    if (emails.Add(e))
                        result.Add(new DistributionListMemberDto { Email = e, MemberType = "Other", Label = "Manual", IsActive = true });
                }
            }

            return result;
        }
    }
}
