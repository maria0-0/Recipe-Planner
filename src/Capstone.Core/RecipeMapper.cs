using System.Text.Json;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core;

public record RecipeDto(
    string? Id,
    string? DisplayName,
    string? Cuisine = "General",
    int TotalTimeMinutes = 30,
    int Servings = 4,
    List<IngredientLine>? Ingredients = null,
    List<string>? Steps = null,
    int? Rating = null,
    DifficultyLevel Difficulty = DifficultyLevel.Medium,
    int PrepTimeMinutes = 10,
    int CookTimeMinutes = 20,
    NutritionInfo? Nutrition = null,
    Diet DietTags = Diet.None,
    decimal? EstimatedCost = null,
    bool IsFavorite = false,
    string? PhotoPath = null,
    string? SourceUrl = null);

/// <summary>
/// Converts Recipe items to JSON and back safely.
/// </summary>
public class RecipeMapper : IItemMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        RecipeDto dto = item switch
        {
            Recipe r => new RecipeDto(
                r.Id,
                r.DisplayName,
                r.Cuisine,
                r.TotalTimeMinutes,
                r.Servings,
                r.Ingredients.ToList(),
                r.Steps.ToList(),
                r.Rating,
                r.Difficulty,
                r.PrepTimeMinutes,
                r.CookTimeMinutes,
                r.Nutrition,
                r.DietTags,
                r.EstimatedCost,
                r.IsFavorite,
                r.PhotoPath,
                r.SourceUrl),

            _ => new RecipeDto(item.Id, item.DisplayName)
        };

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<RecipeDto>(json, JsonOptions);
            if (dto is null)
                return null;

            // An item requires a valid, non-blank Id and DisplayName
            if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.DisplayName))
                return null;

            return new StovetopRecipe(
                dto.Id,
                dto.DisplayName,
                heatLevel: "Medium",
                cuisine: dto.Cuisine ?? "General",
                totalTimeMinutes: dto.TotalTimeMinutes,
                servings: dto.Servings,
                ingredients: dto.Ingredients,
                steps: dto.Steps,
                rating: dto.Rating,
                difficulty: dto.Difficulty,
                prepTimeMinutes: dto.PrepTimeMinutes,
                cookTimeMinutes: dto.CookTimeMinutes,
                nutrition: dto.Nutrition,
                dietTags: dto.DietTags,
                estimatedCost: dto.EstimatedCost,
                isFavorite: dto.IsFavorite,
                photoPath: dto.PhotoPath,
                sourceUrl: dto.SourceUrl);

        }
        catch (Exception)
        {
            return null;
        }
    }
}
