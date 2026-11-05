namespace Capstone.Core.Models;


public class AutoShoppingList
{
    private readonly MealPlan _plan;

    public IReadOnlyList<IngredientLine> Items { get; private set; } = [];

    public AutoShoppingList(MealPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        _plan = plan;

        _plan.PlanChanged += Refresh;

        Refresh();
    }

    public void Refresh()
    {
        Items = _plan.GenerateShoppingList();
    }
}