using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Web.Filters;
using System.Security.Claims;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("barcode")]
public class BarcodeController : Controller
{
    private readonly IBarcodeScanService _barcodeScanService;

    public BarcodeController(IBarcodeScanService barcodeScanService)
    {
        _barcodeScanService = barcodeScanService;
    }

    [HttpGet("")]
    [RateLimit(MaxRequests = 30, TimeWindowMinutes = 1)]
    public async Task<IActionResult> Scan(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return View("~/Views/ProductSearch/BarcodeScanResult.cshtml");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Index", "ProductSearch");
        }

        var result = await _barcodeScanService.ScanBarcodeAsync(barcode, userId);

        TempData["BarcodeSearchTerm"] = barcode;

        return View("~/Views/ProductSearch/BarcodeScanResult.cshtml", result);
    }
}
