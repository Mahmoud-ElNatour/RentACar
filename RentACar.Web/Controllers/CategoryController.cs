using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryManager _categoryManager;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoryController(CategoryManager categoryManager, UserManager<IdentityUser> userManager)
        {
            _categoryManager = categoryManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();
            return View("~/Views/ControlPanel/Category/Index.cshtml", categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("~/Views/ControlPanel/Category/_AddCategory.cshtml", new CategoryDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            await _categoryManager.AddCategoryAsync(dto, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _categoryManager.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Category/_EditCategory.cshtml", cat);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDto dto)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            await _categoryManager.UpdateCategoryAsync(dto, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _categoryManager.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Category/_DeleteCategory.cshtml", cat);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int categoryId)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            await _categoryManager.DeleteCategoryAsync(categoryId, userId);
            return RedirectToAction(nameof(Index));
        }
    }
}
