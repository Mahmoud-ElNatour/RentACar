using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly CategoryManager _categoryManager;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoriesController(CategoryManager categoryManager, UserManager<IdentityUser> userManager)
        {
            _categoryManager = categoryManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User) ?? string.Empty;
                await _categoryManager.AddCategoryAsync(dto, userId);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryManager.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User) ?? string.Empty;
                await _categoryManager.UpdateCategoryAsync(dto, userId);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryManager.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int categoryId)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            await _categoryManager.DeleteCategoryAsync(categoryId, userId);
            return RedirectToAction(nameof(Index));
        }
    }
}
