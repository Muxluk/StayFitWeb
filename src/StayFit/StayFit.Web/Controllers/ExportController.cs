using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[Authorize]
public class ExportController : BaseController
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ExportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(ExportViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        var userEmail = GetCurrentUserEmailOrEmpty();

        var result = await _exportService.ExportFoodLogsAsync(
            userEmail,
            model.From,
            model.To,
            model.Format);

        return MatchResult(result,
            export => File(export.FileBytes, export.ContentType, export.FileName),
            failure =>
            {
                ModelState.AddModelError(string.Empty, failure.ErrorMessage);
                return View("Index", model);
            });
    }
}
