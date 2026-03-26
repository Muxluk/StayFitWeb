using System.ComponentModel.DataAnnotations;

namespace StayFit.Application.DTOs;

public record NutritionGoalDto(
    int Id,
    string UserId,
    float CaloriesGoal,
    float ProteinGoal,
    float FatGoal,
    float CarbsGoal,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class SetNutritionGoalDto
{
    [Display(Name = "Калорії (ккал)")]
    [Range(0, 10000, ErrorMessage = "Значення має бути від 0 до 10000")]
    public float CaloriesGoal { get; set; }

    [Display(Name = "Білки (г)")]
    [Range(0, 10000, ErrorMessage = "Значення має бути від 0 до 10000")]
    public float ProteinGoal { get; set; }

    [Display(Name = "Жири (г)")]
    [Range(0, 10000, ErrorMessage = "Значення має бути від 0 до 10000")]
    public float FatGoal { get; set; }

    [Display(Name = "Вуглеводи (г)")]
    [Range(0, 10000, ErrorMessage = "Значення має бути від 0 до 10000")]
    public float CarbsGoal { get; set; }
}