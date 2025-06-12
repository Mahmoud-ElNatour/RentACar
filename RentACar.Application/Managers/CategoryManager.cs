using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
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
        private readonly UserManager<AspNetUser> _userManager;

        public CategoryManager(ICategoryRepository categoryRepository, IMapper mapper, UserManager<AspNetUser> userManager)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<CategoryDto?> AddCategoryAsync(CategoryDto categoryDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return null; // Or throw UnauthorizedAccessException
            }

            var existingCategory = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (existingCategory != null)
            {
                return null; // Or throw InvalidOperationException
            }

            var categoryEntity = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.AddAsync(categoryEntity);
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

        public async Task<CategoryDto?> UpdateCategoryAsync(CategoryDto categoryDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return null; // Or throw UnauthorizedAccessException
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.CategoryId);
            if (existingCategory == null)
            {
                return null; // Or throw KeyNotFoundException
            }

            var categoryWithNameExists = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (categoryWithNameExists != null && categoryWithNameExists.CategoryId != categoryDto.CategoryId)
            {
                return null; // Or throw InvalidOperationException
            }

            _mapper.Map(categoryDto, existingCategory);
            await _categoryRepository.UpdateAsync(existingCategory);
            return _mapper.Map<CategoryDto>(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return false; // Or throw UnauthorizedAccessException
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return false; // Or throw KeyNotFoundException
            }

            await _categoryRepository.DeleteAsync(id);
            return true;
        }
        public async Task<bool> DeleteCategoryByNameAsync(string name, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return false; // Or throw UnauthorizedAccessException
            }
            var existingCategory = await _categoryRepository.GetByNameAsync(name);
            if (existingCategory == null)
            {
                return false; // Or throw KeyNotFoundException
            }
            await _categoryRepository.DeleteAsync(existingCategory.CategoryId);
            return true;
        }
        public async Task<bool> UpdateCategoryNameAsync(int id, string newName, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return false; // Or throw UnauthorizedAccessException
            }
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return false; // Or throw KeyNotFoundException
            }
            var categoryWithNameExists = await _categoryRepository.GetByNameAsync(newName);
            if (categoryWithNameExists != null && categoryWithNameExists.CategoryId != id)
            {
                return false; // Or throw InvalidOperationException
            }
            existingCategory.Name = newName;
            await _categoryRepository.UpdateAsync(existingCategory);
            return true;
        }

    }
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
        }
    }

}

