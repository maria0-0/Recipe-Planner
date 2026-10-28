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

    public bool RemoveRecipe(string recipeId)
    {
        var recipe = _meals.FirstOrDefault(m => m.Id == recipeId);
        return recipe is not null && _meals.Remove(recipe);
    }

    public void Clear() => _meals.Clear();

    public decimal CalculateEstimatedCost() =>
        _meals.Sum(m => m.EstimatedCost ?? 0m);

    public int TotalCalories() =>
        _meals.Sum(m => m.Nutrition?.Calories ?? 0);

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
