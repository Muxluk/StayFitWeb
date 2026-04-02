using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для вибору категорій продуктів користувачами
/// </summary>
[ApiController]
[Route("api/categories")]
[ApiExplorerSettings(IgnoreApi = false)]
public class CategoryController : BaseController
{
    private readonly ILogger<CategoryController> _logger;
    private readonly IFoodCategoryService _categoryService;

    public CategoryController(
        ILogger<CategoryController> logger,
        IFoodCategoryService categoryService)
    {
        _logger = logger;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Отримати активні категорії для вибору
    /// GET: /api/categories
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetActiveCategories()
    {
        _logger.LogInformation("Запит списку активних категорій");

        var result = await _categoryService.GetActiveAsync();

        return result.Match<IActionResult>(
            onSuccess: success =>
            {
                var categories = success.Data.Select(c => new { id = c.Id, name = c.Name, description = c.Description });
                return Ok(new { data = categories.ToList(), count = categories.Count() });
            },
            onFailure: failure =>
            {
                _logger.LogError("Помилка при завантаженні категорій: {Error}", failure.ErrorMessage);
                return BadRequest(new { error = failure.ErrorMessage });
            }
        );
    }

    /// <summary>
    /// Отримати категорію за ID
    /// GET: /api/categories/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        _logger.LogInformation("Запит категорії #{CategoryId}", id);

        var result = await _categoryService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            onSuccess: success =>
            {
                var category = success.Data;
                return Ok(new { id = category.Id, name = category.Name, description = category.Description });
            },
            onFailure: failure =>
            {
                _logger.LogError("Помилка при завантаженні категорії: {Error}", failure.ErrorMessage);
                return BadRequest(new { error = failure.ErrorMessage });
            }
        );
    }
}
