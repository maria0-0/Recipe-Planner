namespace Capstone.Core.Models;

/// <summary>
/// A recipe representing an item in the meal planner.
/// </summary>
public abstract class Recipe : IRecipe
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Cuisine { get; }
    public int TotalTimeMinutes { get; }
    public int Servings { get; }
    public IReadOnlyList<IngredientLine> Ingredients { get; }
    public IReadOnlyList<string> Steps { get; }
    public int? Rating { get; }
    public DifficultyLevel Difficulty { get; }
    public int PrepTimeMinutes { get; }
    public int CookTimeMinutes { get; }
    public NutritionInfo? Nutrition { get; }
    public Diet DietTags { get; }
    public decimal? EstimatedCost { get; }
    public bool IsFavorite { get; set; }
    public string? PhotoPath { get; set; }
    public string? SourceUrl { get; set; }

    protected Recipe(
        string id,
        string displayName,
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
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Recipe Id cannot be empty or null.", nameof(id));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Recipe DisplayName cannot be empty or null.", nameof(displayName));

        Id = id;
        DisplayName = displayName;
        Cuisine = cuisine;
        TotalTimeMinutes = totalTimeMinutes;
        Servings = servings;
        Ingredients = ingredients ?? [];
        Steps = steps ?? [];
        Rating = rating;
        Difficulty = difficulty;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Nutrition = nutrition;
        DietTags = dietTags;
        EstimatedCost = estimatedCost;
        IsFavorite = isFavorite;
        PhotoPath = photoPath;
        SourceUrl = sourceUrl;
    }

    public abstract string Describe();

    public Recipe Scale(int targetServings)
    {
        if (targetServings <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetServings), "Target servings must be positive.");

        decimal factor = (decimal)targetServings / Servings;
        var scaledIngredients = Ingredients
            .Select(i => new IngredientLine(i.Name, i.Quantity * factor, i.Unit, i.PricePerUnit, i.CaloriesPer100g))
            .ToList();

        return CreateScaled(targetServings, scaledIngredients);
    }

    protected abstract Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients);
}
