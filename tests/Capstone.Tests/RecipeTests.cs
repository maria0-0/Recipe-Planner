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

}



