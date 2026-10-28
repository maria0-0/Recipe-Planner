namespace Capstone.Core.Models;

/// <summary>
/// A recipe that requires no cooking (e.g. salads, gazpacho, overnight oats).
/// </summary>
public class NoCookRecipe : Recipe
{
    public bool RequiresChilling { get; }

    public NoCookRecipe(
        string id,
        string displayName,
        bool requiresChilling = false,
        string cuisine = "General",
        int totalTimeMinutes = 15,
        int servings = 4,
        IReadOnlyList<IngredientLine>? ingredients = null,
        IReadOnlyList<string>? steps = null,
        int? rating = null,
        DifficultyLevel difficulty = DifficultyLevel.Easy,
        int prepTimeMinutes = 15,
        int cookTimeMinutes = 0,
        NutritionInfo? nutrition = null,
        Diet dietTags = Diet.None,
        decimal? estimatedCost = null,
        bool isFavorite = false,
        string? photoPath = null,
        string? sourceUrl = null)
        : base(id, displayName, cuisine, totalTimeMinutes, servings, ingredients, steps, rating,
               difficulty, prepTimeMinutes, cookTimeMinutes, nutrition, dietTags, estimatedCost, isFavorite, photoPath, sourceUrl)
    {
        RequiresChilling = requiresChilling;
    }

    public override string Describe() =>
        $"{DisplayName} · No-cook fresh prep (chilling: {(RequiresChilling ? "yes" : "no")}) in {TotalTimeMinutes} mins";

    protected override Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients) =>
        new NoCookRecipe(
            Id,
            DisplayName,
            RequiresChilling,
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
