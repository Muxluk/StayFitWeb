using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;

namespace StayFit.Infrastructure.Services;

public class UsdaFoodDataClient : IUsdaFoodDataClient
{
    private readonly HttpClient _httpClient;
    private readonly UsdaFoodDataOptions _options;
    private readonly ILogger<UsdaFoodDataClient> _logger;

    public UsdaFoodDataClient(
        HttpClient httpClient,
        IOptions<UsdaFoodDataOptions> options,
        ILogger<UsdaFoodDataClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UsdaSearchResponse?> SearchByBarcodeAsync(string barcode)
    {
        try
        {
            var url = $"{_options.ApiUrl}?query={Uri.EscapeDataString(barcode)}&api_key={_options.ApiKey}&pageSize=1";
            var response = await _httpClient.GetFromJsonAsync<UsdaSearchResponse>(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing barcode search on USDA FoodData Central for barcode {Barcode}", barcode);
            return null;
        }
    }
}
