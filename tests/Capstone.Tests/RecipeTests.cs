using Capstone.Core;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Tests;

public class RecipeTests
{
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Recipe_with_empty_or_whitespace_id_throws_ArgumentException(string? invalidId)
    {
        Assert.Throws<ArgumentException>(() => new StovetopRecipe(invalidId!, "Pancakes"));
    }

    [Fact]
    public void Recipe_with_empty_name_throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new StovetopRecipe("rec-1", "   "));
    }

    [Fact]
    public void Recipe_sets_default_values_when_optional_parameters_omitted()
    {
        var recipe = new StovetopRecipe("rec-10", "Classic Carbonara");

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
        var recipe = new StovetopRecipe("rec-1", "Margherita Pizza");

        category.AddRecipe(recipe);

        Assert.Single(category.Recipes);
        Assert.Equal("Margherita Pizza", category.Recipes[0].DisplayName);
    }

    [Fact]
    public void Adding_duplicate_recipe_id_to_store_throws_and_does_not_modify_count()
    {
        var store = new RecipeStore();
        store.Add(new StovetopRecipe("rec-1", "Risotto"));

        Assert.Throws<ArgumentException>(() => store.Add(new StovetopRecipe("rec-1", "Another Risotto")));
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void Removing_recipe_from_store_returns_true_and_decreases_count()
    {
        var store = new RecipeStore();
        var recipe = new StovetopRecipe("rec-1", "Lasagna");
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
        var recipe = new StovetopRecipe("rec-1", "Pasta", servings: 4, ingredients: ingredients);

        var scaled = recipe.Scale(6);

        Assert.Equal(6, scaled.Servings);
        Assert.Equal(300m, scaled.Ingredients[0].Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Scaling_to_zero_or_negative_servings_throws(int invalidServings)
    {
        var recipe = new StovetopRecipe("rec-1", "Pasta", servings: 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => recipe.Scale(invalidServings));
    }

    [Fact]
    public void Shopping_list_combines_quantities_of_same_ingredient_and_unit()
    {
        var plan = new MealPlan();
        var recipe1 = new StovetopRecipe("rec-1", "Lunch", ingredients: [new("Pasta", 200m, Unit.Gram)]);
        var recipe2 = new StovetopRecipe("rec-2", "Dinner", ingredients: [new("Pasta", 300m, Unit.Gram)]);

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
        var recipe1 = new StovetopRecipe("rec-1", "Bread", ingredients: [new("Flour", 200m, Unit.Gram)]);
        var recipe2 = new StovetopRecipe("rec-2", "Pancakes", ingredients: [new("Flour", 1m, Unit.Cup)]);

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

    [Fact]
    public void Recipe_preserves_new_metadata_when_scaled()
    {
        var nutrition = new NutritionInfo(450, 25m, 40m, 15m);
        var recipe = new StovetopRecipe(
            "rec-scaled",
            "Tofu Stir Fry",
            servings: 2,
            ingredients: [new("Tofu", 200m, Unit.Gram, PricePerUnit: 1.5m, CaloriesPer100g: 76m)],
            difficulty: DifficultyLevel.Easy,
            prepTimeMinutes: 10,
            cookTimeMinutes: 15,
            nutrition: nutrition,
            dietTags: Diet.Vegetarian | Diet.Vegan,
            estimatedCost: 8.5m,
            isFavorite: true,
            photoPath: "photos/tofu.jpg",
            sourceUrl: "https://tiktok.com/@chef/video/123");

        var scaled = recipe.Scale(4);

        Assert.Equal(4, scaled.Servings);
        Assert.Equal(400m, scaled.Ingredients[0].Quantity);
        Assert.Equal(1.5m, scaled.Ingredients[0].PricePerUnit);
        Assert.Equal(76m, scaled.Ingredients[0].CaloriesPer100g);
        Assert.Equal(DifficultyLevel.Easy, scaled.Difficulty);
        Assert.Equal(10, scaled.PrepTimeMinutes);
        Assert.Equal(15, scaled.CookTimeMinutes);
        Assert.Equal(nutrition, scaled.Nutrition);
        Assert.True(scaled.DietTags.HasFlag(Diet.Vegetarian));
        Assert.True(scaled.DietTags.HasFlag(Diet.Vegan));
        Assert.False(scaled.DietTags.HasFlag(Diet.Keto));
        Assert.Equal(8.5m, scaled.EstimatedCost);
        Assert.True(scaled.IsFavorite);
        Assert.Equal("photos/tofu.jpg", scaled.PhotoPath);
        Assert.Equal("https://tiktok.com/@chef/video/123", scaled.SourceUrl);
    }

    [Fact]
    public void MealPlan_calculates_total_calories_and_estimated_cost()
    {
        var plan = new MealPlan();
        var r1 = new StovetopRecipe("r1", "Dish 1", nutrition: new NutritionInfo(300, 10m, 20m, 5m), estimatedCost: 12.50m);
        var r2 = new BakedRecipe("r2", "Dish 2", nutrition: new NutritionInfo(550, 30m, 50m, 15m), estimatedCost: 15.00m);

        plan.AddRecipe(r1);
        plan.AddRecipe(r2);

        Assert.Equal(850, plan.TotalCalories());
        Assert.Equal(27.50m, plan.CalculateEstimatedCost());

        Assert.True(plan.RemoveRecipe("r1"));
        Assert.Single(plan.Meals);
        Assert.Equal(550, plan.TotalCalories());
        Assert.Equal(15.00m, plan.CalculateEstimatedCost());
    }

    [Fact]
    public void Category_supports_season_and_removal()
    {
        var category = new Category("Summer Refreshers", "Cold drinks and salads", Season.Summer);
        var recipe = new NoCookRecipe("salad-1", "Watermelon Salad");

        category.AddRecipe(recipe);

        Assert.Equal(Season.Summer, category.Season);
        Assert.Single(category.Recipes);

        var removed = category.RemoveRecipe("salad-1");
        Assert.True(removed);
        Assert.Empty(category.Recipes);
    }

    [Fact]
    public void RecipeStore_supports_foreach_and_indexers()
    {
        var store = new RecipeStore();
        var r1 = new StovetopRecipe("r-1", "Oatmeal");
        var r2 = new BakedRecipe("r-2", "Apple Pie");
        store.Add(r1);
        store.Add(r2);

        // Position indexer
        Assert.Same(r1, store[0]);
        Assert.Same(r2, store[1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => store[2]);

        // ID indexer
        Assert.Same(r1, store["r-1"]);
        Assert.Null(store["unknown-id"]);

        // foreach enumeration directly on store
        var iterated = new List<IItem>();
        foreach (var item in store)
        {
            iterated.Add(item);
        }

        Assert.Equal(2, iterated.Count);
        Assert.Same(r1, iterated[0]);
        Assert.Same(r2, iterated[1]);
    }

    [Fact]
    public void RecipeStore_Matching_is_lazy_and_case_insensitive()
    {
        var store = new RecipeStore();
        store.Add(new StovetopRecipe("r-1", "Pasta Carbonara"));

        var query = store.Matching("pasta");

        // Add item after query definition (tests deferred execution)
        store.Add(new BakedRecipe("r-2", "Pasta Bake"));

        var matches = query.ToList();
        Assert.Equal(2, matches.Count);
        Assert.Contains(matches, m => m.Id == "r-1");
        Assert.Contains(matches, m => m.Id == "r-2");
    }

    [Fact]
    public void RecentlyUsed_tracks_recently_cooked_recipes_with_lru_behavior()
    {
        var recent = new RecentlyUsed<string>(capacity: 3);

        recent.Push("Carbonara");
        recent.Push("Pancakes");
        recent.Push("Lasagna");

        // Newest first
        Assert.Equal(["Lasagna", "Pancakes", "Carbonara"], recent.Items);

        // Pushing existing item moves it to front without duplicating
        recent.Push("Carbonara");
        Assert.Equal(["Carbonara", "Lasagna", "Pancakes"], recent.Items);

        // Pushing 4th item drops oldest (Pancakes)
        recent.Push("Risotto");
        Assert.Equal(["Risotto", "Carbonara", "Lasagna"], recent.Items);
    }
}



