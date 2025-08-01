using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

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
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();
            return Ok(categories);
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
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto)
        {
            _logger.LogInformation("Creating category");
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var created = await _categoryManager.AddCategoryAsync(dto, userId);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.CategoryId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
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
            var success = await _categoryManager.DeleteCategoryAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
