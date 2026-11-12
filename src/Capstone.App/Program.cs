using Capstone.Core;
using Capstone.Core.Models;
using SharpQuest.Contracts;

Console.WriteLine("=== Recipe Planner — Session 6 Events Demo ===\n");

// 1. The Publisher (The Recipe Book Store)
var book = new RecipeStore();

// 2. Subscriber 1: Counter (keeps a running count)
int totalAdded = 0;
book.ItemAdded += item =>
{
    totalAdded++;
    Console.WriteLine($"[Subscriber 1 - Counter] Running total recipes: {totalAdded}");
};

// 3. Subscriber 2: Recently added tracker 
var recentlyAdded = new RecentlyUsed<IItem>(capacity: 3);
book.ItemAdded += item =>
{
    recentlyAdded.Push(item);
    Console.WriteLine($"[Subscriber 2 - RecentTracker] Added '{item.DisplayName}' to recent items (holds: {recentlyAdded.Count})");
};

// 4. Trigger the event by adding recipes
Console.WriteLine("Adding recipes to the book...\n");
book.Add(new StovetopRecipe(
    "r-1", "Spaghetti Carbonara",
    cuisine: "Italian",
    ingredients: [
        new("Pasta", 200, Unit.Gram),
        new("Eggs", 2, Unit.Piece),
        new("Bacon", 100, Unit.Gram)
    ]));

book.Add(new BakedRecipe(
    "r-2", "Apple Crumble",
    cuisine: "British",
    dietTags: Diet.Vegetarian,
    ingredients: [
        new("Apples", 3, Unit.Piece),
        new("Flour", 200, Unit.Gram),
        new("Sugar", 100, Unit.Gram)
    ]));

book.Add(new NoCookRecipe(
    "r-3", "Greek Salad",
    cuisine: "Greek",
    dietTags: Diet.Vegetarian | Diet.GlutenFree,
    ingredients: [
        new("Tomatoes", 2, Unit.Piece),
        new("Cucumbers", 1, Unit.Piece),
        new("Feta Cheese", 150, Unit.Gram)
    ]));

// 5. Demonstrate function passed as a parameter
Console.WriteLine("\n--- Function as Parameter (Filtering with Where) ---");
var quickRecipes = book.Matching("Salad");
foreach (var r in quickRecipes)
{
    Console.WriteLine($"Found: {r.DisplayName}");
}

// --- Meal Plan Change Audit (Rich Event Payload) ---
Console.WriteLine("\n--- Meal Plan Change Audit ---");
var plan = new MealPlan();

plan.PlanChangedDetailed += change =>
{
    Console.WriteLine($"[Plan Audit] Action: {change.Action} | Recipe: '{change.RecipeName}' at {change.Timestamp:HH:mm:ss}");
};

plan.AddRecipe(new StovetopRecipe("r-1", "Spaghetti Carbonara"));
plan.AddRecipe(new BakedRecipe("r-2", "Apple Crumble"));
plan.RemoveRecipe("r-1");

// ======================================================================
// === SESSION 7 — LINQ QUERIES DEMO ====================================
// ======================================================================
Console.WriteLine("\n=== Session 7 — LINQ Queries Demo ===\n");

// 1. Deferred Execution demo: the query is defined BEFORE adding a recipe!
var lazyNames = book.Select(r => r.DisplayName);
Console.WriteLine($"Lazy query created. Items in book: {book.Count}");
book.Add(new StovetopRecipe(
    "r-4", "Banana Pancakes",
    cuisine: "American",
    dietTags: Diet.Vegetarian,
    ingredients: [new("Banana", 1, Unit.Piece), new("Eggs", 2, Unit.Piece)]));
Console.WriteLine($"Added 'Banana Pancakes'. Query evaluated now -> Found {lazyNames.Count()} recipes:\n  {string.Join(", ", lazyNames)}");

// 2. The first page (2 items at a time, in name order)
Console.WriteLine("\nThe first page (2 at a time, name order):");
foreach (var item in book.Page(skip: 0, take: 2))
{
    Console.WriteLine($"  • {item.DisplayName}");
}

// 3. The longest names (our Longest extension method!)
Console.WriteLine("\nThe 2 longest recipe names (our Longest extension method):");
foreach (var item in book.Longest(2))
{
    Console.WriteLine($"  • {item.DisplayName} ({item.DisplayName.Length} chars)");
}

// 4. Summary statistics (our Summary extension method!)
var stats = book.Summary();
Console.WriteLine($"\nSummary: {stats.Count} recipes · {stats.DistinctInitials} different initials · longest \"{stats.LongestName}\" · {stats.AverageNameLength:F1} chars on average");

// 5. Fridge Mode: "What can I cook right now?"
var myFridge = new[] { "Banana", "Eggs", "Pasta" };
Console.WriteLine($"\nFridge Mode (I have: {string.Join(", ", myFridge)}):");

Console.WriteLine("a) Ready to cook right now (0 missing ingredients):");
foreach (var r in book.OfType<Recipe>().FridgeMatch(myFridge, maxMissing: 0))
{
    Console.WriteLine($"  ✓ {r.DisplayName} — Ready to cook! (all ingredients in fridge)");
}

Console.WriteLine("\nb) Almost ready (up to 1 missing ingredient):");
foreach (var r in book.OfType<Recipe>().FridgeMatch(myFridge, maxMissing: 1))
{
    var missing = r.MissingIngredients(myFridge);
    if (missing.Count == 0)
    {
        Console.WriteLine($"  ✓ {r.DisplayName} — Ready to cook! (0 missing)");
    }
    else
    {
        Console.WriteLine($"  ✓ {r.DisplayName} — Missing {missing.Count} item: {string.Join(", ", missing)}");
    }
}

// 6. Multi-criteria Filtering (by diet, max time, ingredient, or cuisine)
Console.WriteLine("\n--- Multi-Criteria Search Demo ---");

Console.WriteLine("a) Search by ingredient + max cook time (ingredient: 'Eggs', max 30 mins):");
foreach (var r in book.OfType<Recipe>().Filter(ingredient: "Eggs", maxMinutes: 30))
{
    Console.WriteLine($"  ✓ {r.DisplayName} ({r.TotalTimeMinutes} mins, Cuisine: {r.Cuisine})");
}

Console.WriteLine("\nb) Search by diet only (Vegetarian, any cook time):");
foreach (var r in book.OfType<Recipe>().Filter(diet: Diet.Vegetarian))
{
    Console.WriteLine($"  ✓ {r.DisplayName} ({r.TotalTimeMinutes} mins, Diet: {r.DietTags})");
}

Console.WriteLine("\nc) Search by cuisine ('Italian'):");
foreach (var r in book.OfType<Recipe>().Filter(cuisine: "Italian"))
{
    Console.WriteLine($"  ✓ {r.DisplayName} ({r.TotalTimeMinutes} mins)");
}

// 7. Custom domain vocabulary & Cuisine GroupBy
Console.WriteLine($"\nAll recipes in book (OneLine): {book.OneLine()}");
Console.WriteLine($"Is book empty: {book.IsEmpty()}");
Console.WriteLine("Average cooking time per cuisine:");
foreach (var (cuisine, avgMinutes) in book.OfType<Recipe>().AverageTimePerCuisine())
{
    Console.WriteLine($"  • {cuisine}: {avgMinutes:F1} mins");
}