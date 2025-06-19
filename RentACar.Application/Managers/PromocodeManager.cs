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

        public PromocodeManager(IPromocodeRepository promocodeRepository, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<PromocodeManager> logger)
        {
            _promocodeRepository = promocodeRepository;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
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
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Normal employees might only see active ones
                var activePromocodes = await _promocodeRepository.GetActiveAsync();
                return _mapper.Map<List<PromocodeDto>>(activePromocodes);
            }

            // Admins see all
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

            _mapper.Map(promocodeDto, existingPromocode);
            await _promocodeRepository.UpdateAsync(existingPromocode);
            _logger.LogInformation("Promocode {Id} updated", promocodeDto.PromocodeId);
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

            var existingPromocode = await _promocodeRepository.GetByIdAsync(id);
            if (existingPromocode == null)
            {
                _logger.LogWarning("Promocode {Id} not found", id);
                return false; // Or throw KeyNotFoundException
            }

            var promocodeToDelete = _mapper.Map<Promocode>(await _promocodeRepository.GetByIdAsync(id));
            await _promocodeRepository.DeleteAsync(promocodeToDelete);
            _logger.LogInformation("Promocode {Id} deleted", id);
            return true;
        }
    }

    public class PromocodeProfile : Profile
    {
        public PromocodeProfile()
        {
            CreateMap<Promocode, PromocodeDto>().ReverseMap();
        }
    }

}