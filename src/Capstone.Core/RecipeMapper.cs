using System.Text.Json;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core;

public record RecipeDto(
    string? Id,
    string? DisplayName,
    string? Kind = "stovetop",
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
    string? SourceUrl = null,
    int? OvenTemperature = null,
    string? HeatLevel = null,
    bool? RequiresChilling = null);


public class RecipeMapper : IItemMapper
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static RecipeDto ToDto(IItem item) => item switch
    {
        BakedRecipe b => new RecipeDto(
            b.Id,
            b.DisplayName,
            Kind: "baked",
            b.Cuisine,
            b.TotalTimeMinutes,
            b.Servings,
            b.Ingredients.ToList(),
            b.Steps.ToList(),
            b.Rating,
            b.Difficulty,
            b.PrepTimeMinutes,
            b.CookTimeMinutes,
            b.Nutrition,
            b.DietTags,
            b.EstimatedCost,
            b.IsFavorite,
            b.PhotoPath,
            b.SourceUrl,
            OvenTemperature: b.OvenTemperature),

        StovetopRecipe s => new RecipeDto(
            s.Id,
            s.DisplayName,
            Kind: "stovetop",
            s.Cuisine,
            s.TotalTimeMinutes,
            s.Servings,
            s.Ingredients.ToList(),
            s.Steps.ToList(),
            s.Rating,
            s.Difficulty,
            s.PrepTimeMinutes,
            s.CookTimeMinutes,
            s.Nutrition,
            s.DietTags,
            s.EstimatedCost,
            s.IsFavorite,
            s.PhotoPath,
            s.SourceUrl,
            HeatLevel: s.HeatLevel),

        NoCookRecipe n => new RecipeDto(
            n.Id,
            n.DisplayName,
            Kind: "no-cook",
            n.Cuisine,
            n.TotalTimeMinutes,
            n.Servings,
            n.Ingredients.ToList(),
            n.Steps.ToList(),
            n.Rating,
            n.Difficulty,
            n.PrepTimeMinutes,
            n.CookTimeMinutes,
            n.Nutrition,
            n.DietTags,
            n.EstimatedCost,
            n.IsFavorite,
            n.PhotoPath,
            n.SourceUrl,
            RequiresChilling: n.RequiresChilling),

        Recipe r => new RecipeDto(
            r.Id,
            r.DisplayName,
            Kind: "stovetop",
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

        _ => new RecipeDto(item.Id, item.DisplayName, Kind: "stovetop")
    };

    public static IItem? ToItem(RecipeDto dto)
    {
        if (dto is null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.DisplayName))
            return null;

        var kind = dto.Kind?.ToLowerInvariant();

        if (kind == "baked" || dto.OvenTemperature.HasValue)
        {
            return new BakedRecipe(
                dto.Id,
                dto.DisplayName,
                ovenTemperature: dto.OvenTemperature ?? 180,
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

        if (kind == "no-cook" || dto.RequiresChilling.HasValue)
        {
            return new NoCookRecipe(
                dto.Id,
                dto.DisplayName,
                requiresChilling: dto.RequiresChilling ?? false,
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

        return new StovetopRecipe(
            dto.Id,
            dto.DisplayName,
            heatLevel: dto.HeatLevel ?? "Medium",
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

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var dto = ToDto(item);
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<RecipeDto>(json, JsonOptions);
            return dto is null ? null : ToItem(dto);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
