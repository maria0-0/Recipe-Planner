namespace Capstone.Core.Models;


public record PlanChangeArgs(string Action, string RecipeName, DateTime Timestamp);

public class MealPlan
{
    private readonly List<Recipe> _meals = [];

    public IReadOnlyList<Recipe> Meals => _meals;

    public event Action? PlanChanged;

    public event Action<PlanChangeArgs>? PlanChangedDetailed;

    public void AddRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        _meals.Add(recipe);
        PlanChanged?.Invoke();
        PlanChangedDetailed?.Invoke(new PlanChangeArgs("ADDED", recipe.DisplayName, DateTime.Now));
    }

     public bool RemoveRecipe(string recipeId)
    {
        var recipe = _meals.FirstOrDefault(m => m.Id == recipeId);
        if (recipe is not null && _meals.Remove(recipe))
        {
            PlanChanged?.Invoke();
            PlanChangedDetailed?.Invoke(new PlanChangeArgs("REMOVED", recipe.DisplayName, DateTime.Now));
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _meals.Clear();
        PlanChanged?.Invoke();
    }

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
