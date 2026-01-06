using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetUser = RentACar.Application.DTOs.AspNetUser;

namespace RentACar.Application.Managers
{
    public class CategoryManager
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CategoryManager> _logger;


        private readonly AuditLogManager _auditLogManager;

        public CategoryManager(ICategoryRepository categoryRepository, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<CategoryManager> logger, AuditLogManager auditLogManager)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _auditLogManager = auditLogManager;
        }

        public async Task<CategoryDto?> AddCategoryAsync(CategoryDto categoryDto, string userId)
        {
            _logger.LogInformation("Adding category {@Category}", categoryDto);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to add categories", userId);
                return null;
            }

            var existingCategory = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (existingCategory != null)
            {
                _logger.LogWarning("Category name {Name} already exists", categoryDto.Name);
                return null; 
            }

            var categoryEntity = _mapper.Map<Category>(categoryDto);
            
            // Handle Image Upload
            if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await categoryDto.ImageFile.CopyToAsync(memoryStream);
                    categoryEntity.Image = memoryStream.ToArray();
                }
            }

            await _categoryRepository.AddAsync(categoryEntity);

            _logger.LogInformation("Category added with id {Id}", categoryEntity.CategoryId);
            await _auditLogManager.LogAsync("Create", "Category", categoryEntity.CategoryId.ToString(), $"Added category: {categoryDto.Name}");
            return _mapper.Map<CategoryDto>(categoryEntity);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto?> GetCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameAsync(name);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<CategoryDto>> GetAllActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<CategoryListDto>> GetAllCategoriesForListAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryListDto>>(categories);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(CategoryDto categoryDto, string userId)
        {
            _logger.LogInformation("Updating category {Id}", categoryDto.CategoryId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to update categories", userId);
                return null;
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.CategoryId);
            if (existingCategory == null)
            {
                _logger.LogWarning("Category {Id} not found", categoryDto.CategoryId);
                return null;
            }

            var categoryWithNameExists = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (categoryWithNameExists != null && categoryWithNameExists.CategoryId != categoryDto.CategoryId)
            {
                _logger.LogWarning("Category name {Name} already exists", categoryDto.Name);
                return null;
            }

            // Map properties but handle Image differently
            _mapper.Map(categoryDto, existingCategory);

            // Handle Image Upload if provided
            if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await categoryDto.ImageFile.CopyToAsync(memoryStream);
                    existingCategory.Image = memoryStream.ToArray();
                }
            }
            // If ImageFile is null, we keep the existing Image (which is consistent with standard partial update logic, though mapper might have overwritten it if we are not careful. 
            // `_mapper.Map(dto, entity)` usually overwrites. Since DTO doesn't have `Image` property (I removed it from DTO in previous step - wait, I removed ImageUrl but didn't add byte[] Image to DTO), 
            // so `Image` property on Entity should be untouched by Mapper if DTO doesn't have it.
            // Wait, I need to check if I updated AutoMapper profile correctly.

            await _categoryRepository.UpdateAsync(existingCategory);

            _logger.LogInformation("Category {Id} updated", categoryDto.CategoryId);
            await _auditLogManager.LogAsync("Update", "Category", categoryDto.CategoryId.ToString(), $"Updated category: {categoryDto.Name}");
            return _mapper.Map<CategoryDto>(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            _logger.LogInformation("Deleting category {Id}", id);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to delete category", userId);
                return false; // Or throw UnauthorizedAccessException
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Category {Id} not found", id);
                return false; // Or throw KeyNotFoundException
            }

            var hasCars = await _categoryRepository.HasCarsAsync(id);
            if (hasCars)
            {
                // Soft delete
                existingCategory.IsActive = false;
                await _categoryRepository.UpdateAsync(existingCategory);

                // Cascade soft delete to cars
                await _categoryRepository.DeactivateCarsAsync(id);

                _logger.LogInformation("Category {Id} soft deleted (has related cars)", id);
                await _auditLogManager.LogAsync("Delete", "Category", id.ToString(), "Soft deleted category (has cars) and deactivated associated cars");
            }
            else
            {
                // Hard delete
                await _categoryRepository.DeleteAsync(id);
                _logger.LogInformation("Category {Id} hard deleted (no related cars)", id);
                await _auditLogManager.LogAsync("Delete", "Category", id.ToString(), "Hard deleted category");
            }
            
            return true;
        }
        public async Task<bool> DeleteCategoryByNameAsync(string name, string userId)
        {
            _logger.LogInformation("Deleting category {Name}", name);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to delete category", userId);
                return false; // Or throw UnauthorizedAccessException
            }
            var existingCategory = await _categoryRepository.GetByNameAsync(name);
            if (existingCategory == null)
            {
                _logger.LogWarning("Category {Name} not found", name);
                return false; // Or throw KeyNotFoundException
            }
            // Soft delete
            existingCategory.IsActive = false;
            await _categoryRepository.UpdateAsync(existingCategory);

            _logger.LogInformation("Category {Name} soft deleted", name);
            await _auditLogManager.LogAsync("Delete", "Category", existingCategory.CategoryId.ToString(), $"Soft deleted category by name: {name}");
            return true;
        }
        public async Task<bool> UpdateCategoryNameAsync(int id, string newName, string userId)
        {
            _logger.LogInformation("Updating category {Id} name", id);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to update category", userId);
                return false; // Or throw UnauthorizedAccessException
            }
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Category {Id} not found", id);
                return false; // Or throw KeyNotFoundException
            }
            var categoryWithNameExists = await _categoryRepository.GetByNameAsync(newName);
            if (categoryWithNameExists != null && categoryWithNameExists.CategoryId != id)
            {
                _logger.LogWarning("Category name {Name} already exists", newName);
                return false; // Or throw InvalidOperationException
            }
            existingCategory.Name = newName;
            await _categoryRepository.UpdateAsync(existingCategory);

            _logger.LogInformation("Category {Id} name updated", id);
            await _auditLogManager.LogAsync("Update", "Category", id.ToString(), $"Renamed category to: {newName}");
            return true;
        }

    }
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CarsCount, opt => opt.MapFrom(src => src.Cars.Count))
                .ForMember(dest => dest.ImageBase64, opt => opt.MapFrom(src => src.Image != null ? Convert.ToBase64String(src.Image) : null))
                .ReverseMap()
                .ForMember(dest => dest.Image, opt => opt.Ignore()); // Ignore Image on reverse map, handle manually

            CreateMap<Category, CategoryListDto>()
                .ForMember(dest => dest.CarsCount, opt => opt.MapFrom(src => src.Cars.Count));
        }
    }

}

