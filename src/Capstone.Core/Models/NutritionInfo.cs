namespace Capstone.Core.Models;

/// <summary>
/// Nutritional information per serving of a recipe.
/// </summary>
public record NutritionInfo(
    int Calories,
    decimal ProteinGrams,
    decimal CarbsGrams,
    decimal FatGrams);
