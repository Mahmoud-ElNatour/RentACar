using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly CategoryManager _categoryManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryManager categoryManager, UserManager<IdentityUser> userManager, ILogger<CategoryController> logger)
        {
            _categoryManager = categoryManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("~/Category")]
        [Authorize(Roles = "Admin,Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Category/Index.cshtml");
        }

        [HttpGet("~/Category/Add")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Category/_AddCategory.cshtml", new CategoryDto());
        }

        [HttpGet("~/Category/Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var cat = await _categoryManager.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Category/_EditCategory.cshtml", cat);
        }

        [HttpGet("~/Category/Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var cat = await _categoryManager.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Category/_DeleteCategory.cshtml", cat);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<CategoryListDto>>> Get()
        {
            var categories = await _categoryManager.GetAllCategoriesForListAsync();
            return Ok(categories);
        }

        [HttpGet("Image/{id}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetImage(int id)
        {
            var category = await _categoryManager.GetCategoryByIdAsync(id);
            if (category == null || string.IsNullOrEmpty(category.ImageBase64))
            {
                // Serve a default placeholder or return 404
                return NotFound("Image not found");
                // Alternatively serve a static file:
                // return File(System.IO.File.ReadAllBytes("wwwroot/images/default.jpg"), "image/jpeg");
            }
            
            // The DTO has ImageBase64 string. We need to convert it back to bytes to serve as file.
            // Ideally Manager should return bytes for this specific purpose, but GetCategoryByIdAsync returns DTO.
            // Let's rely on the fact that we have the Base64 in DTO.
            try 
            {
                var bytes = Convert.FromBase64String(category.ImageBase64);
                return File(bytes, "image/jpeg");
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<CategoryDto>> Get(int id)
        {
            var cat = await _categoryManager.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return Ok(cat);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> Create([FromForm] CategoryDto dto)
        {
            _logger.LogInformation("Creating category");
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var created = await _categoryManager.AddCategoryAsync(dto, userId);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.CategoryId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] CategoryDto dto)
        {
            if (id != dto.CategoryId) return BadRequest();
            _logger.LogInformation("Updating category {Id}", id);
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var updated = await _categoryManager.UpdateCategoryAsync(dto, userId);
            if (updated == null) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting category {Id}", id);
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            try
            {
                var success = await _categoryManager.DeleteCategoryAsync(id, userId);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint prevented deleting category {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Unable to delete category because related records exist. Remove the related data before deleting the category.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting category {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while deleting the category. Please try again later.");
            }
        }
    }
}
