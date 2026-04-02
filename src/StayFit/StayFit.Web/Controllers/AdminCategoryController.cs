using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для управління категоріями продуктів (тільки для адміністраторів)
/// </summary>
[Authorize(Roles = "Admin")]
[Route("admin/categories")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AdminCategoryController : BaseController
{
    private readonly IFoodCategoryService _categoryService;
    private readonly ILogger<AdminCategoryController> _logger;

    public AdminCategoryController(IFoodCategoryService categoryService, ILogger<AdminCategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Список всіх категорій
    /// GET: /admin/categories
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Адміністратор {AdminId} переглядає список категорій", userId);

        var result = await _categoryService.GetAllAsync();
        
        return MatchResult(
            result,
            onSuccess: categories => View(categories.ToList()),
            onFailure: failure =>
            {
                _logger.LogError("Помилка при завантаженні списку категорій: {Error}", failure.ErrorMessage);
                return View(new List<object>());
            }
        );
    }

    /// <summary>
    /// Форма для створення нової категорії
    /// GET: /admin/categories/create
    /// </summary>
    [HttpGet("create")]
    public IActionResult Create()
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Адміністратор {AdminId} відкрив форму створення категорії", userId);
        return View();
    }

    /// <summary>
    /// Зберегти нову категорію
    /// POST: /admin/categories/create
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Адміністратор {AdminId} створює категорію: {CategoryName}", userId, request.Name);

        var result = await _categoryService.CreateAsync(request.Name, request.Description);
        
        return MatchResult(
            result,
            onSuccess: category =>
            {
                TempData["Success"] = "Категорія успішно створена";
                return RedirectToAction(nameof(Index));
            },
            onFailure: failure =>
            {
                ModelState.AddModelError("", failure.ErrorMessage);
                return View(request);
            }
        );
    }

    /// <summary>
    /// Форма для редагування категорії
    /// GET: /admin/categories/edit/{id}
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Адміністратор {AdminId} редагує категорію #{CategoryId}", userId, id);

        var result = await _categoryService.GetByIdAsync(id);
        
        return MatchResult(
            result,
            onSuccess: category => View(new EditCategoryRequest
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            }),
            onFailure: failure =>
            {
                TempData["Error"] = "Категорія не знайдена";
                return RedirectToAction(nameof(Index));
            }
        );
    }

    /// <summary>
    /// Зберегти зміни категорії
    /// POST: /admin/categories/edit/{id}
    /// </summary>
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Адміністратор {AdminId} оновлює категорію #{CategoryId}", userId, id);

        var result = await _categoryService.UpdateAsync(id, request.Name, request.Description, request.IsActive);
        
        return MatchResult(
            result,
            onSuccess: category =>
            {
                TempData["Success"] = "Категорія успішно оновлена";
                return RedirectToAction(nameof(Index));
            },
            onFailure: failure =>
            {
                ModelState.AddModelError("", failure.ErrorMessage);
                return View(request);
            }
        );
    }

    /// <summary>
    /// Видалити категорію
    /// POST: /admin/categories/delete/{id}
    /// </summary>
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Адміністратор {AdminId} видаляє категорію #{CategoryId}", userId, id);

        var result = await _categoryService.DeleteAsync(id);
        
        result.Match(
            onSuccess: _ =>
            {
                TempData["Success"] = "Категорія успішно видалена";
            },
            onSuccessWithData: _ =>
            {
                // Не використовується для DeleteAsync
            },
            onFailure: failure =>
            {
                TempData["Error"] = failure.ErrorMessage;
            }
        );

        return RedirectToAction(nameof(Index));
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class EditCategoryRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
