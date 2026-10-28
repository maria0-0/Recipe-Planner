namespace Capstone.Core.Models;

public class BakedRecipe : Recipe
{
    public int OvenTemperature { get; }

    public BakedRecipe(
        string id,
        string displayName,
        int ovenTemperature = 180,
        string cuisine = "General",
        int totalTimeMinutes = 45,
        int servings = 4,
        IReadOnlyList<IngredientLine>? ingredients = null,
        IReadOnlyList<string>? steps = null,
        int? rating = null,
        DifficultyLevel difficulty = DifficultyLevel.Medium,
        int prepTimeMinutes = 15,
        int cookTimeMinutes = 30,
        NutritionInfo? nutrition = null,
        Diet dietTags = Diet.None,
        decimal? estimatedCost = null,
        bool isFavorite = false,
        string? photoPath = null,
        string? sourceUrl = null)
        : base(id, displayName, cuisine, totalTimeMinutes, servings, ingredients, steps, rating,
               difficulty, prepTimeMinutes, cookTimeMinutes, nutrition, dietTags, estimatedCost, isFavorite, photoPath, sourceUrl)
    {
        if (ovenTemperature <= 0)
            throw new ArgumentOutOfRangeException(nameof(ovenTemperature), "Oven temperature must be positive.");

        OvenTemperature = ovenTemperature;
    }

    public override string Describe() =>
        $"{DisplayName} · Baked at {OvenTemperature}°C for {TotalTimeMinutes} mins";

    protected override Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients) =>
        new BakedRecipe(
            Id,
            DisplayName,
            OvenTemperature,
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
