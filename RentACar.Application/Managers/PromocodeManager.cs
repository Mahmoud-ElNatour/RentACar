using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUser = RentACar.Application.DTOs.AspNetUser;

namespace RentACar.Application.Managers
{
    public class PromocodeManager
    {
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;

        public PromocodeManager(IPromocodeRepository promocodeRepository, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _promocodeRepository = promocodeRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<PromocodeDto?> AddPromocodeAsync(PromocodeDto promocodeDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return null; // Or throw UnauthorizedAccessException
            }

            var existingPromocode = await _promocodeRepository.GetByNameAsync(promocodeDto.Name);
            if (existingPromocode != null)
            {
                return null; // Or throw InvalidOperationException
            }

            var promocodeEntity = _mapper.Map<Promocode>(promocodeDto);
            var addedEntity = await _promocodeRepository.AddAsync(promocodeEntity);
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return null; // Or throw UnauthorizedAccessException
            }

            var existingPromocode = await _promocodeRepository.GetByIdAsync(promocodeDto.PromocodeId);
            if (existingPromocode == null)
            {
                return null; // Or throw KeyNotFoundException
            }

            var promocodeWithNameExists = await _promocodeRepository.GetByNameAsync(promocodeDto.Name);
            if (promocodeWithNameExists != null && promocodeWithNameExists.PromocodeId != promocodeDto.PromocodeId)
            {
                return null; // Or throw InvalidOperationException
            }

            _mapper.Map(promocodeDto, existingPromocode);
            await _promocodeRepository.UpdateAsync(existingPromocode);
            return _mapper.Map<PromocodeDto>(existingPromocode);
        }

        public async Task<bool> DeletePromocodeAsync(int id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return false; // Or throw UnauthorizedAccessException
            }

            var existingPromocode = await _promocodeRepository.GetByIdAsync(id);
            if (existingPromocode == null)
            {
                return false; // Or throw KeyNotFoundException
            }

            var promocodeToDelete = _mapper.Map<Promocode>(await _promocodeRepository.GetByIdAsync(id));
            await _promocodeRepository.DeleteAsync(promocodeToDelete);
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