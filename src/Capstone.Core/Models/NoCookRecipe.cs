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
        int? rating = null)
        : base(id, displayName, cuisine, totalTimeMinutes, servings, ingredients, steps, rating)
    {
        RequiresChilling = requiresChilling;
    }

    public override string Describe() =>
        $"{DisplayName} · No-cook fresh prep (chilling: {(RequiresChilling ? "yes" : "no")}) in {TotalTimeMinutes} mins";

    protected override Recipe CreateScaled(int targetServings, IReadOnlyList<IngredientLine> scaledIngredients) =>
        new NoCookRecipe(Id, DisplayName, RequiresChilling, Cuisine, TotalTimeMinutes, targetServings, scaledIngredients, Steps, Rating);
}
