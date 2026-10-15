namespace Capstone.Core.Models;

public class MealPlan
{
    private readonly List<Recipe> _meals = [];

    public IReadOnlyList<Recipe> Meals => _meals;

    public void AddRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        _meals.Add(recipe);
    }

    public IReadOnlyList<IngredientLine> GenerateShoppingList()
    {
        return _meals
            .SelectMany(m => m.Ingredients)
            .GroupBy(i => (i.Name.Trim().ToLowerInvariant(), i.Unit))
            .Select(g => new IngredientLine(
                g.First().Name,
                g.Sum(i => i.Quantity),
                g.Key.Unit))
            .ToList();
    }
}
