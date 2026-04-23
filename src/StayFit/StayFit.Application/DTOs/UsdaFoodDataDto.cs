using System.Text.Json.Serialization;

namespace StayFit.Application.DTOs;

public class UsdaSearchResponse
{
    [JsonPropertyName("foods")]
    public List<UsdaFood>? Foods { get; set; }
}

public class UsdaFood
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("brandOwner")]
    public string? BrandOwner { get; set; }

    [JsonPropertyName("foodNutrients")]
    public List<UsdaNutrient>? FoodNutrients { get; set; }
}

public class UsdaNutrient
{
    [JsonPropertyName("nutrientName")]
    public string? NutrientName { get; set; }

    [JsonPropertyName("value")]
    public float? Value { get; set; }
}
