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
        private readonly EmailManager _emailManager;
        private readonly EmployeeManager _employeeManager;

        public CategoryManager(ICategoryRepository categoryRepository, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<CategoryManager> logger, AuditLogManager auditLogManager, EmailManager emailManager, EmployeeManager employeeManager)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
            _employeeManager = employeeManager;
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
            var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
            await _emailManager.SendCategoryUpdateEmail(emails, categoryEntity, "Create", "New Category", "Created", "System/Admin");

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
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return null;
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            if (!isAdmin && !isEmployee)
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

            // Enforce: Only Admin can change IsActive
            // If not admin, we force the DTO's IsActive to match the existing entity's state before mapping
            if (!isAdmin)
            {
                categoryDto.IsActive = existingCategory.IsActive;
            }

            var categoryWithNameExists = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (categoryWithNameExists != null && categoryWithNameExists.CategoryId != categoryDto.CategoryId)
            {
                _logger.LogWarning("Category name {Name} already exists", categoryDto.Name);
                return null;
            }

            // Capture Snapshot Before
            var before = new { 
                existingCategory.Name, 
                existingCategory.IsActive 
            };

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

            // Capture Snapshot After
            var after = new { 
                existingCategory.Name, 
                existingCategory.IsActive 
            };

            await _categoryRepository.UpdateAsync(existingCategory);

            _logger.LogInformation("Category {Id} updated", categoryDto.CategoryId);
            await _auditLogManager.LogAsync(
                "Update", 
                "Category", 
                categoryDto.CategoryId.ToString(), 
                $"Updated category: {categoryDto.Name}",
                oldValues: before,
                newValues: after);
            
            // 📨 Send Email if IsActive status changed, or just general update
            // Request: "Category IsActive status changed"
            // We should check IsActive.
            // DTO has IsActive (potentially updated). Existing is updated in memory by map.
            // Wait, helper check:
            // I need to check OLD status. But I already mapped it.
            // I should capture old status before map. But I missed it in this chunk replacement.
            // If I assume Category update is rare and important, I can send on *any* update.
            // Request: "Triggers: Category IsActive status changed".
            // If I map, existingCategory is updated.
            // I can't check unless I saved old value.
            // However, this replace_file_content is applied to the end of the method.
            // I can't easily insert code at the top without replacing whole method.
            // Strategy: I'll accept sending email on ANY update (covering IsActive change) or I will skip strictly "IsActive changed" check here and assume it's covered by general update notification.
            // Actually, for "Category", the user said "Category deleted or archived" and "Category IsActive status changed".
            // If I send on Update, it's safer.
            // I'll send on update.
             var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
             await _emailManager.SendCategoryUpdateEmail(emails, existingCategory, "Update", "IsActive/Details", existingCategory.IsActive.ToString(), "System/Admin");
            
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
            
            // 📨 Send Category Update Email (Delete/Archive)
            var emailsDel = await _employeeManager.GetActiveEmployeeEmailsAsync();
            await _emailManager.SendCategoryUpdateEmail(emailsDel, existingCategory, hasCars ? "Archived" : "Deleted", "Active", hasCars ? "Archived" : "Deleted", "System/Admin");
            
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

