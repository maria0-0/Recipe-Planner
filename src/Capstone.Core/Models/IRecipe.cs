using SharpQuest.Contracts;

namespace Capstone.Core.Models;

/// <summary>
/// Contract for a recipe in the meal planner.
/// </summary>
public interface IRecipe : IItem
{
    string Cuisine { get; }
    int TotalTimeMinutes { get; }
    int Servings { get; }
    IReadOnlyList<IngredientLine> Ingredients { get; }
    IReadOnlyList<string> Steps { get; }
    int? Rating { get; }
    DifficultyLevel Difficulty { get; }
    int PrepTimeMinutes { get; }
    int CookTimeMinutes { get; }
    NutritionInfo? Nutrition { get; }
    Diet DietTags { get; }
    decimal? EstimatedCost { get; }
    bool IsFavorite { get; set; }
    string? PhotoPath { get; set; }
    string? SourceUrl { get; set; }
}
