namespace Capstone.Core.Models;

public class StovetopRecipe : Recipe
{
    public string HeatLevel { get; }

    public StovetopRecipe(
        string id,
        string displayName,
        string heatLevel = "Medium",
        string cuisine = "General",
        int totalTimeMinutes = 30,
        int servings = 4,
        IReadOnlyList<IngredientLine>? ingredients = null,
        IReadOnlyList<string>? steps = null,
        int? rating = null,
        DifficultyLevel difficulty = DifficultyLevel.Medium,
        int prepTimeMinutes = 10,
        int cookTimeMinutes = 20,
        NutritionInfo? nutrition = null,
        Diet dietTags = Diet.None,
        decimal? estimatedCost = null,
        bool isFavorite = false,
        string? photoPath = null,
        string? sourceUrl = null)
        : base(id, displayName, cuisine, totalTimeMinutes, servings, ingredients, steps, rating,
               difficulty, prepTimeMinutes, cookTimeMinutes, nutrition, dietTags, estimatedCost, isFavorite, photoPath, sourceUrl)
    {
        HeatLevel = string.IsNullOrWhiteSpace(heatLevel) ? "Medium" : heatLevel;
    }

    public override string Describe() =>
        $"{DisplayName} · Stovetop on {HeatLevel} heat for {TotalTimeMinutes} mins";

    protected override Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients) =>
        new StovetopRecipe(
            Id,
            DisplayName,
            HeatLevel,
            Cuisine,
            TotalTimeMinutes,
            targetServings,
            scaledIngredients,
            Steps,
            Rating,
            Difficulty,
            PrepTimeMinutes,
            CookTimeMinutes,
            Nutrition,
            DietTags,
            EstimatedCost,
            IsFavorite,
            PhotoPath,
            SourceUrl);
}
