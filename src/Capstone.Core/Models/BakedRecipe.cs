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
        int? rating = null)
        : base(id, displayName, cuisine, totalTimeMinutes, servings, ingredients, steps, rating)
    {
        if (ovenTemperature <= 0)
            throw new ArgumentOutOfRangeException(nameof(ovenTemperature), "Oven temperature must be positive.");

        OvenTemperature = ovenTemperature;
    }

    public override string Describe() =>
        $"{DisplayName} · Baked at {OvenTemperature}°C for {TotalTimeMinutes} mins";

    protected override Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients) =>
        new BakedRecipe(Id, DisplayName, OvenTemperature, Cuisine, TotalTimeMinutes, targetServings, scaledIngredients, Steps, Rating);
}
