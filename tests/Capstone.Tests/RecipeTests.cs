using Capstone.Core;
using Capstone.Core.Models;
using Capstone.Core.Storage;
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

    [Fact]
    public void RecipeStore_ItemAdded_event_notifies_multiple_subscribers()
    {
        var store = new RecipeStore();
        int count = 0;
        string? lastAdded = null;

        store.ItemAdded += item => count++;
        store.ItemAdded += item => lastAdded = item.DisplayName;

        store.Add(new StovetopRecipe("r-1", "Risotto"));

        Assert.Equal(1, count);
        Assert.Equal("Risotto", lastAdded);
    }

    [Fact]
    public void RecipeStore_ItemAdded_does_not_fire_when_add_fails()
    {
        var store = new RecipeStore();
        store.Add(new StovetopRecipe("r-1", "Risotto"));

        int eventCount = 0;
        store.ItemAdded += _ => eventCount++;

        // Duplicate ID throws ArgumentException
        Assert.Throws<ArgumentException>(() => store.Add(new StovetopRecipe("r-1", "Duplicate Risotto")));
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void AutoShoppingList_updates_itself_automatically_when_plan_changes()
    {
        var plan = new MealPlan();
        var autoList = new AutoShoppingList(plan);

        Assert.Empty(autoList.Items);

        // Adding a recipe to the plan updates the shopping list automatically without manual calls
        plan.AddRecipe(new StovetopRecipe("r-1", "Lunch", ingredients: [new("Pasta", 200m, Unit.Gram)]));
        Assert.Single(autoList.Items);
        Assert.Equal(200m, autoList.Items[0].Quantity);

        // Adding another recipe with the same ingredient accumulates the quantity automatically
        plan.AddRecipe(new StovetopRecipe("r-2", "Dinner", ingredients: [new("Pasta", 300m, Unit.Gram)]));
        Assert.Single(autoList.Items);
        Assert.Equal(500m, autoList.Items[0].Quantity);

        // Removing a recipe automatically decreases the shopping list quantity
        plan.RemoveRecipe("r-1");
        Assert.Single(autoList.Items);
        Assert.Equal(300m, autoList.Items[0].Quantity);
    }

    [Fact]
    public void RecipeStore_fires_FavoriteToggled_and_CookingLogged_events()
    {
        var store = new RecipeStore();
        var recipe = new StovetopRecipe("r-1", "Pancakes");

        bool favoriteFired = false;
        bool cookingFired = false;

        store.FavoriteToggled += r => favoriteFired = true;
        store.CookingLogged += r => cookingFired = true;

        store.ToggleFavorite(recipe);
        store.LogCooking(recipe);

        Assert.True(favoriteFired);
        Assert.True(recipe.IsFavorite);
        Assert.True(cookingFired);
    }

    [Fact]
    public void MealPlan_PlanChangedDetailed_fires_with_rich_payload()
    {
        var plan = new MealPlan();
        PlanChangeArgs? lastChange = null;

        plan.PlanChangedDetailed += change => lastChange = change;

        var recipe = new StovetopRecipe("r-1", "Carbonara");
        plan.AddRecipe(recipe);

        Assert.NotNull(lastChange);
        Assert.Equal("ADDED", lastChange.Action);
        Assert.Equal("Carbonara", lastChange.RecipeName);

        plan.RemoveRecipe("r-1");
        Assert.Equal("REMOVED", lastChange.Action);
        Assert.Equal("Carbonara", lastChange.RecipeName);
    }

    [Fact]
    public void RecipeQueries_Page_returns_correct_slice_in_name_order()
    {
        var recipes = new List<Recipe>
        {
            new StovetopRecipe("r-3", "Carbonara"),
            new BakedRecipe("r-1", "Apple Pie"),
            new NoCookRecipe("r-2", "Banana Smoothie"),
            new StovetopRecipe("r-4", "Donut")
        };

        // Name order: Apple Pie (0), Banana Smoothie (1), Carbonara (2), Donut (3)
        var page = recipes.Page(skip: 1, take: 2);

        Assert.Equal(2, page.Count);
        Assert.Equal("Banana Smoothie", page[0].DisplayName);
        Assert.Equal("Carbonara", page[1].DisplayName);
    }

    [Fact]
    public void RecipeQueries_FridgeMatch_finds_recipes_matching_fridge_ingredients()
    {
        var r1 = new StovetopRecipe("r-1", "Omelette", ingredients: [
            new("Eggs", 2, Unit.Piece),
            new("Cheese", 50, Unit.Gram)
        ]);

        var r2 = new StovetopRecipe("r-2", "Complex Stew", ingredients: [
            new("Beef", 500, Unit.Gram),
            new("Carrot", 2, Unit.Piece),
            new("Potato", 3, Unit.Piece),
            new("Wine", 200, Unit.Milliliter)
        ]);

        var recipes = new List<Recipe> { r1, r2 };

        // I have eggs, cheese, and milk in my fridge
        var fridge = new List<string> { "Eggs", "Cheese", "Milk" };

        var matches = recipes.FridgeMatch(fridge, maxMissing: 0).ToList();

        Assert.Single(matches);
        Assert.Equal("Omelette", matches[0].DisplayName);

        // Test MissingIngredients
        Assert.Empty(r1.MissingIngredients(fridge));

        var stewMissing = r2.MissingIngredients(fridge);
        Assert.Equal(4, stewMissing.Count);
        Assert.Contains("Beef", stewMissing);
        Assert.Contains("Carrot", stewMissing);
    }

    [Fact]
    public void RecipeQueries_Filter_multi_criteria_search()
    {
        var chickenSalad = new NoCookRecipe(
            "r-1",
            "Chicken Salad",
            cuisine: "American",
            totalTimeMinutes: 15,
            dietTags: Diet.GlutenFree,
            ingredients: [new("Chicken Breast", 200, Unit.Gram), new("Lettuce", 100, Unit.Gram)]);

        var chickenCurry = new StovetopRecipe(
            "r-2",
            "Chicken Curry",
            cuisine: "Indian",
            totalTimeMinutes: 45,
            dietTags: Diet.GlutenFree | Diet.DairyFree,
            ingredients: [new("Chicken Breast", 400, Unit.Gram), new("Curry Paste", 50, Unit.Gram)]);

        var veggiePasta = new StovetopRecipe(
            "r-3",
            "Veggie Pasta",
            cuisine: "Italian",
            totalTimeMinutes: 20,
            dietTags: Diet.Vegetarian,
            ingredients: [new("Pasta", 200, Unit.Gram), new("Tomato Sauce", 100, Unit.Milliliter)]);

        var recipes = new List<Recipe> { chickenSalad, chickenCurry, veggiePasta };

        // 1. Filter by ingredient only ("Chicken") - any time, any diet
        var chickenRecipes = recipes.Filter(ingredient: "Chicken").ToList();
        Assert.Equal(2, chickenRecipes.Count);

        // 2. Filter by diet only (Vegetarian) - any time
        var vegRecipes = recipes.Filter(diet: Diet.Vegetarian).ToList();
        Assert.Single(vegRecipes);
        Assert.Equal("Veggie Pasta", vegRecipes[0].DisplayName);

        // 3. Multi-criteria: Contains chicken + max 20 minutes
        var quickChicken = recipes.Filter(ingredient: "Chicken", maxMinutes: 20).ToList();
        Assert.Single(quickChicken);
        Assert.Equal("Chicken Salad", quickChicken[0].DisplayName);

        // 4. Multi-criteria: Indian cuisine + DairyFree
        var indianDairyFree = recipes.Filter(cuisine: "Indian", diet: Diet.DairyFree).ToList();
        Assert.Single(indianDairyFree);
        Assert.Equal("Chicken Curry", indianDairyFree[0].DisplayName);
    }

    [Fact]
    public void RecipeQueries_MostUsedIngredients_aggregates_across_all_recipes()
    {
        var r1 = new StovetopRecipe("r-1", "Dish 1", ingredients: [
            new("Salt", 1, Unit.Teaspoon),
            new("Pepper", 1, Unit.Teaspoon),
            new("Olive Oil", 15, Unit.Milliliter)
        ]);

        var r2 = new StovetopRecipe("r-2", "Dish 2", ingredients: [
            new("Olive Oil", 30, Unit.Milliliter),
            new("Garlic", 2, Unit.Piece),
            new("Salt", 2, Unit.Teaspoon)
        ]);

        var recipes = new List<Recipe> { r1, r2 };

        var top = recipes.MostUsedIngredients(top: 2);

        Assert.Equal(2, top.Count);
        // Salt and Olive Oil are both used in 2 recipes
        Assert.Contains(top, t => t.Ingredient == "salt" && t.Count == 2);
        Assert.Contains(top, t => t.Ingredient == "olive oil" && t.Count == 2);
    }

    [Fact]
    public void RecipeQueries_Domain_vocabulary_and_cuisine_grouping()
    {
        var emptyList = new List<IItem>();
        Assert.True(emptyList.IsEmpty());

        var r1 = new StovetopRecipe("r-1", "Pasta Carbonara", cuisine: "Italian", totalTimeMinutes: 20);
        var r2 = new BakedRecipe("r-2", "Pizza Margherita", cuisine: "Italian", totalTimeMinutes: 40);
        var r3 = new StovetopRecipe("r-3", "Tacos", cuisine: "Mexican", totalTimeMinutes: 15);

        var recipes = new List<Recipe> { r1, r2, r3 };

        // Test IsEmpty and OneLine
        Assert.False(recipes.IsEmpty());
        Assert.Equal("Pasta Carbonara, Pizza Margherita, Tacos", recipes.OneLine());

        // Test GroupBy cuisine averages
        var cuisineStats = recipes.AverageTimePerCuisine();
        Assert.Equal(2, cuisineStats.Count);

        var italian = Assert.Single(cuisineStats, c => c.Cuisine == "Italian");
        Assert.Equal(30.0, italian.AverageMinutes); // (20 + 40) / 2 = 30

        var mexican = Assert.Single(cuisineStats, c => c.Cuisine == "Mexican");
        Assert.Equal(15.0, mexican.AverageMinutes);
    }

    [Fact]
    public async Task RecipeFile_round_trip_preserves_all_recipe_kinds_and_properties()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"recipes-test-{Guid.NewGuid():N}.json");
        try
        {
            var store = new RecipeStore();
            var baked = new BakedRecipe(
                "b-1", "Apple Pie",
                ovenTemperature: 190,
                cuisine: "American",
                totalTimeMinutes: 50,
                ingredients: [new("Apples", 4, Unit.Piece), new("Flour", 300, Unit.Gram)],
                steps: ["Mix", "Bake"]);

            var stovetop = new StovetopRecipe(
                "s-1", "Stir Fry",
                heatLevel: "High",
                cuisine: "Asian",
                totalTimeMinutes: 20,
                dietTags: Diet.Vegetarian);

            var nocook = new NoCookRecipe(
                "n-1", "Gazpacho",
                requiresChilling: true,
                cuisine: "Spanish",
                totalTimeMinutes: 15,
                dietTags: Diet.Vegan);

            store.Add(baked);
            store.Add(stovetop);
            store.Add(nocook);

            await RecipeFile.SaveAsync(store, tempFile);

            var loaded = await RecipeFile.LoadAsync(tempFile);

            Assert.Equal(3, loaded.Count);

            var loadedBaked = Assert.IsType<BakedRecipe>(loaded.FindById("b-1"));
            Assert.Equal("Apple Pie", loadedBaked.DisplayName);
            Assert.Equal(190, loadedBaked.OvenTemperature);
            Assert.Equal(2, loadedBaked.Ingredients.Count);

            var loadedStovetop = Assert.IsType<StovetopRecipe>(loaded.FindById("s-1"));
            Assert.Equal("High", loadedStovetop.HeatLevel);
            Assert.Equal(Diet.Vegetarian, loadedStovetop.DietTags);

            var loadedNoCook = Assert.IsType<NoCookRecipe>(loaded.FindById("n-1"));
            Assert.True(loadedNoCook.RequiresChilling);
            Assert.Equal(Diet.Vegan, loadedNoCook.DietTags);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RecipeFile_missing_file_returns_empty_store()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        var store = await RecipeFile.LoadAsync(missingPath);

        Assert.NotNull(store);
        Assert.Equal(0, store.Count);
        Assert.False(File.Exists(missingPath));
    }

    [Fact]
    public async Task RecipeFile_corrupted_file_throws_InvalidDataException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"corrupt-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{ this is not valid json");
            await Assert.ThrowsAsync<InvalidDataException>(() => RecipeFile.LoadAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RecipeFile_MealPlan_round_trip_persists_meals_and_shopping_list()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"plan-{Guid.NewGuid():N}.json");
        try
        {
            var plan = new MealPlan();
            plan.AddRecipe(new StovetopRecipe("r-1", "Omelette", ingredients: [new("Eggs", 3, Unit.Piece)]));
            plan.AddRecipe(new BakedRecipe("r-2", "Bread", ingredients: [new("Flour", 500, Unit.Gram)]));

            await RecipeFile.SaveMealPlanAsync(plan, tempFile);

            var loadedPlan = await RecipeFile.LoadMealPlanAsync(tempFile);

            Assert.Equal(2, loadedPlan.Meals.Count);
            Assert.Equal(2, loadedPlan.GenerateShoppingList().Count);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RecipeFile_Fridge_round_trip_saves_and_loads_ingredients()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"fridge-{Guid.NewGuid():N}.json");
        try
        {
            var fridge = new List<string> { "Eggs", "Milk", "Cheese", "Butter" };

            await RecipeFile.SaveFridgeAsync(fridge, tempFile);

            var loadedFridge = await RecipeFile.LoadFridgeAsync(tempFile);

            Assert.Equal(4, loadedFridge.Count);
            Assert.Contains("Milk", loadedFridge);
            Assert.Contains("Cheese", loadedFridge);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RecipeFile_CookingHistory_and_Favorites_survive_persistence()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"history-{Guid.NewGuid():N}.json");
        try
        {
            var store = new RecipeStore();
            var fav = new StovetopRecipe("r-1", "Pasta", isFavorite: true);
            var regular = new BakedRecipe("r-2", "Cake", isFavorite: false);

            store.Add(fav);
            store.Add(regular);

            Assert.Single(store.Favorites);
            Assert.Equal("Pasta", store.Favorites[0].DisplayName);

            store.LogCooking(fav, new DateTime(2026, 11, 19, 12, 0, 0));
            Assert.Single(store.CookingHistory);

            await RecipeFile.SaveCookingHistoryAsync(store.CookingHistory, tempFile);

            var loadedHistory = await RecipeFile.LoadCookingHistoryAsync(tempFile);
            Assert.Single(loadedHistory);
            Assert.Equal("Pasta", loadedHistory[0].RecipeName);
            Assert.Equal("r-1", loadedHistory[0].RecipeId);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RecipeFile_Favorites_dedicated_round_trip()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"favs-{Guid.NewGuid():N}.json");
        try
        {
            var favList = new List<Recipe>
            {
                new StovetopRecipe("r-1", "Carbonara", isFavorite: true),
                new BakedRecipe("r-2", "Apple Pie", isFavorite: true)
            };

            await RecipeFile.SaveFavoritesAsync(favList, tempFile);

            var loadedFavs = await RecipeFile.LoadFavoritesAsync(tempFile);

            Assert.Equal(2, loadedFavs.Count);
            Assert.All(loadedFavs, r => Assert.True(r.IsFavorite));
            Assert.Contains(loadedFavs, r => r.DisplayName == "Carbonara");
            Assert.Contains(loadedFavs, r => r.DisplayName == "Apple Pie");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}



