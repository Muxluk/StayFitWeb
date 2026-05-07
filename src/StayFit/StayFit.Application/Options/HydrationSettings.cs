namespace StayFit.Application.Options;

public class HydrationSettings
{
    public int WeightMultiplierMl { get; set; } = 35;
    public List<int> QuickAddButtons { get; set; } = new() { 200, 300, 500 };
}