using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUser = RentACar.Application.DTOs.AspNetUser;
using Microsoft.Extensions.Logging;

namespace RentACar.Application.Managers
{
    public class PromocodeManager
    {
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PromocodeManager> _logger;


        private readonly AuditLogManager _auditLogManager;
        private readonly EmailManager _emailManager;
        private readonly EmployeeManager _employeeManager;

        public PromocodeManager(IPromocodeRepository promocodeRepository, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<PromocodeManager> logger, AuditLogManager auditLogManager, EmailManager emailManager, EmployeeManager employeeManager)
        {
            _promocodeRepository = promocodeRepository;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
            _employeeManager = employeeManager;
        }

        public async Task<PromocodeDto?> AddPromocodeAsync(PromocodeDto promocodeDto, string userId)
        {
            _logger.LogInformation("Adding promocode {Name}", promocodeDto.Name);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return null; // Or throw UnauthorizedAccessException
            }

            var existingPromocode = await _promocodeRepository.GetByNameAsync(promocodeDto.Name);
            if (existingPromocode != null)
            {
                _logger.LogWarning("Promocode {Name} already exists", promocodeDto.Name);
                return null; // Or throw InvalidOperationException
            }

            var promocodeEntity = _mapper.Map<Promocode>(promocodeDto);
            var addedEntity = await _promocodeRepository.AddAsync(promocodeEntity);

            _logger.LogInformation("Promocode added with id {Id}", addedEntity.PromocodeId);
            await _auditLogManager.LogAsync("Create", "Promocode", addedEntity.PromocodeId.ToString(), $"Added promocode: {promocodeDto.Name} ({promocodeDto.DiscountPercentage}%)");
            
            var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
            await _emailManager.SendPromocodeUpdateEmail(emails, addedEntity, "Create", "New Promocode", "System/Admin");

            return _mapper.Map<PromocodeDto>(addedEntity);
        }

        public async Task<PromocodeDto?> GetPromocodeByIdAsync(int id)
        {
            var promocode = await _promocodeRepository.GetByIdAsync(id);
            return _mapper.Map<PromocodeDto>(promocode);
        }

        public async Task<PromocodeDto?> GetPromocodeByNameAsync(string name)
        {
            var promocode = await _promocodeRepository.GetByNameAsync(name);
            return _mapper.Map<PromocodeDto>(promocode);
        }

        public async Task<List<PromocodeDto>> GetAllPromocodesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin)
            {
                // Non-admin users should only see active promocodes
                var activePromocodes = await _promocodeRepository.GetActiveAsync();
                return _mapper.Map<List<PromocodeDto>>(activePromocodes);
            }

            // Admins see all promocodes, including inactive ones
            var allPromocodes = await _promocodeRepository.GetAllAsync();
            return _mapper.Map<List<PromocodeDto>>(allPromocodes);
        }

        public async Task<PromocodeDto?> UpdatePromocodeAsync(PromocodeDto promocodeDto, string userId)
        {
            _logger.LogInformation("Updating promocode {Id}", promocodeDto.PromocodeId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return null; // Or throw UnauthorizedAccessException
            }

            var existingPromocode = await _promocodeRepository.GetByIdAsync(promocodeDto.PromocodeId);
            if (existingPromocode == null)
            {
                _logger.LogWarning("Promocode {Id} not found", promocodeDto.PromocodeId);
                return null; // Or throw KeyNotFoundException
            }

            var promocodeWithNameExists = await _promocodeRepository.GetByNameAsync(promocodeDto.Name);
            if (promocodeWithNameExists != null && promocodeWithNameExists.PromocodeId != promocodeDto.PromocodeId)
            {
                _logger.LogWarning("Promocode name {Name} already exists", promocodeDto.Name);
                return null; // Or throw InvalidOperationException
            }

            var oldIsActive = existingPromocode.IsActive;
            
            _mapper.Map(promocodeDto, existingPromocode);
            await _promocodeRepository.UpdateAsync(existingPromocode);
            
            _logger.LogInformation("Promocode {Id} updated", promocodeDto.PromocodeId);
            await _auditLogManager.LogAsync("Update", "Promocode", promocodeDto.PromocodeId.ToString(), $"Updated promocode: {promocodeDto.Name}");
            
            // Determine reason
            string reason = "General Update";
            if (oldIsActive != existingPromocode.IsActive)
            {
                reason = existingPromocode.IsActive ? "Activated" : "Deactivated";
            }

            // 📨 Send internal notification
            var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
            await _emailManager.SendPromocodeUpdateEmail(emails, existingPromocode, "Update", reason, "System/Admin");
            
            return _mapper.Map<PromocodeDto>(existingPromocode);
        }

        public async Task<bool> DeletePromocodeAsync(int id, string userId)
        {
            _logger.LogInformation("Deleting promocode {Id}", id);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to delete promocode", userId);
                return false; // Or throw UnauthorizedAccessException
            }

            var promocode = await _promocodeRepository.GetByIdAsync(id);
            if (promocode == null)
            {
                _logger.LogWarning("Promocode {Id} not found", id);
                return false; // Or throw KeyNotFoundException
            }

            // Ensure bookings is not null
            var usageCount = promocode.Bookings?.Count ?? 0;

            if (usageCount > 0)
            {
                // Soft Delete: Promocode has been used
                promocode.IsActive = false;
                await _promocodeRepository.UpdateAsync(promocode);
                
                _logger.LogInformation("Promocode {Id} soft deleted (Usage: {Count})", id, usageCount);
                await _auditLogManager.LogAsync("Delete", "Promocode", id.ToString(), $"Soft deleted promocode {id} (Used {usageCount} times)");
                
                // 📨 Send internal notification (Deactivated)
                var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
                await _emailManager.SendPromocodeUpdateEmail(emails, promocode, "Deactivated", "Soft Deleted (Used)", "System/Admin");
            }
            else
            {
                // Hard Delete: Promocode has never been used
                await _promocodeRepository.DeleteAsync(promocode);
                
                _logger.LogInformation("Promocode {Id} hard deleted (Unused)", id);
                await _auditLogManager.LogAsync("Delete", "Promocode", id.ToString(), $"Hard deleted unused promocode {id}");
                
                // 📨 Send internal notification (Deleted)
                var emails2 = await _employeeManager.GetActiveEmployeeEmailsAsync();
                await _emailManager.SendPromocodeUpdateEmail(emails2, promocode, "Deleted", "Hard Deleted (Unused)", "System/Admin");
            }

            return true;
        }
    }

    public class PromocodeProfile : Profile
    {
        public PromocodeProfile()
        {
            CreateMap<Promocode, PromocodeDto>()
                .ForMember(d => d.UsageCount, o => o.MapFrom(s => s.Bookings.Count))
                .ReverseMap();
        }
    }

}