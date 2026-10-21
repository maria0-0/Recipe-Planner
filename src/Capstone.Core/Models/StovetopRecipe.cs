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
        int? rating = null)
        : base(id, displayName, cuisine, totalTimeMinutes, servings, ingredients, steps, rating)
    {
        HeatLevel = string.IsNullOrWhiteSpace(heatLevel) ? "Medium" : heatLevel;
    }

    public override string Describe() =>
        $"{DisplayName} · Stovetop on {HeatLevel} heat for {TotalTimeMinutes} mins";

    protected override Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients) =>
        new StovetopRecipe(Id, DisplayName, HeatLevel, Cuisine, TotalTimeMinutes, targetServings, scaledIngredients, Steps, Rating);
}
