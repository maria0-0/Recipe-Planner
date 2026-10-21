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

    protected Recipe(
        string id,
        string displayName,
        string cuisine = "General",
        int totalTimeMinutes = 30,
        int servings = 4,
        IReadOnlyList<IngredientLine>? ingredients = null,
        IReadOnlyList<string>? steps = null,
        int? rating = null)
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
    }

    public abstract string Describe();

    public Recipe Scale(int targetServings)
    {
        if (targetServings <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetServings), "Target servings must be positive.");

        decimal factor = (decimal)targetServings / Servings;
        var scaledIngredients = Ingredients
        .Select(i => new IngredientLine(i.Name, i.Quantity * factor, i.Unit))
        .ToList();

        return CreateScaled(targetServings, scaledIngredients);
    }

    protected abstract Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients);
}
