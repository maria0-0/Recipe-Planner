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
}
