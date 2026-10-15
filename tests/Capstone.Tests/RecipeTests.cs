using Capstone.Core;
using Capstone.Core.Models;

namespace Capstone.Tests;

public class RecipeTests
{
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Recipe_with_empty_or_whitespace_id_throws_ArgumentException(string? invalidId)
    {
        Assert.Throws<ArgumentException>(() => new Recipe(invalidId!, "Pancakes"));
    }

    [Fact]
    public void Recipe_with_empty_name_throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Recipe("rec-1", "   "));
    }

    [Fact]
    public void Recipe_sets_default_values_when_optional_parameters_omitted()
    {
        var recipe = new Recipe("rec-10", "Classic Carbonara");

        Assert.Equal("General", recipe.Cuisine);
        Assert.Equal(30, recipe.TotalTimeMinutes);
        Assert.Equal(4, recipe.Servings);
        Assert.Empty(recipe.Ingredients);
        Assert.Empty(recipe.Steps);
    }

    [Fact]
    public void Category_can_add_recipes()
    {
        var category = new Category("Italian");
        var recipe = new Recipe("rec-1", "Margherita Pizza");

        category.AddRecipe(recipe);

        Assert.Single(category.Recipes);
        Assert.Equal("Margherita Pizza", category.Recipes[0].DisplayName);
    }

    [Fact]
    public void Adding_duplicate_recipe_id_to_store_throws_and_does_not_modify_count()
    {
        var store = new RecipeStore();
        store.Add(new Recipe("rec-1", "Risotto"));

        Assert.Throws<ArgumentException>(() => store.Add(new Recipe("rec-1", "Another Risotto")));
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void Removing_recipe_from_store_returns_true_and_decreases_count()
    {
        var store = new RecipeStore();
        var recipe = new Recipe("rec-1", "Lasagna");
        store.Add(recipe);

        var removed = store.Remove("rec-1");

        Assert.True(removed);
        Assert.Equal(0, store.Count);
        Assert.Null(store.FindById("rec-1"));
    }

    [Fact]
    public void Category_with_empty_name_throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Category("   "));
    }

    [Fact]
    public void Scaling_recipe_multiplies_ingredient_quantities_by_factor()
    {
        var ingredients = new List<IngredientLine>
        {
            new("Pasta", 200m, Unit.Gram)
        };
        var recipe = new Recipe("rec-1", "Pasta", servings: 4, ingredients: ingredients);

        var scaled = recipe.Scale(6);

        Assert.Equal(6, scaled.Servings);
        Assert.Equal(300m, scaled.Ingredients[0].Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Scaling_to_zero_or_negative_servings_throws(int invalidServings)
    {
        var recipe = new Recipe("rec-1", "Pasta", servings: 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => recipe.Scale(invalidServings));
    }

    [Fact]
    public void Shopping_list_combines_quantities_of_same_ingredient_and_unit()
    {
        var plan = new MealPlan();
        var recipe1 = new Recipe("rec-1", "Lunch", ingredients: [new("Pasta", 200m, Unit.Gram)]);
        var recipe2 = new Recipe("rec-2", "Dinner", ingredients: [new("Pasta", 300m, Unit.Gram)]);

        plan.AddRecipe(recipe1);
        plan.AddRecipe(recipe2);

        var list = plan.GenerateShoppingList();

        Assert.Single(list);
        Assert.Equal(500m, list[0].Quantity);
        Assert.Equal(Unit.Gram, list[0].Unit);
    }

    [Fact]
    public void Shopping_list_keeps_different_units_of_same_ingredient_separate()
    {
        var plan = new MealPlan();
        var recipe1 = new Recipe("rec-1", "Bread", ingredients: [new("Flour", 200m, Unit.Gram)]);
        var recipe2 = new Recipe("rec-2", "Pancakes", ingredients: [new("Flour", 1m, Unit.Cup)]);

        plan.AddRecipe(recipe1);
        plan.AddRecipe(recipe2);

        var list = plan.GenerateShoppingList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.Unit == Unit.Gram && i.Quantity == 200m);
        Assert.Contains(list, i => i.Unit == Unit.Cup && i.Quantity == 1m);
    }

    [Fact]
    public void Empty_meal_plan_generates_empty_shopping_list()
    {
        var plan = new MealPlan();

        var list = plan.GenerateShoppingList();

        Assert.Empty(list);
    }


}



