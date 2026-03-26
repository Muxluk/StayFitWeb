using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[Authorize]
public class ExportController : Controller
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

        var userEmail = User.Identity?.Name ?? string.Empty;

        var result = await _exportService.ExportFoodLogsAsync(
            userEmail,
            model.From,
            model.To,
            model.Format);

        if (result.IsFailure)
        {
            var failure = (StayFit.Domain.Results.Result<StayFit.Application.Interfaces.ExportResult>.Failure)result;
            ModelState.AddModelError(string.Empty, failure.ErrorMessage);
            return View("Index", model);
        }

        var success = (StayFit.Domain.Results.Result<StayFit.Application.Interfaces.ExportResult>.Success)result;
        var export = success.Data;

        return File(export.FileBytes, export.ContentType, export.FileName);
    }
}
