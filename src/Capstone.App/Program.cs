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
book.Add(new StovetopRecipe("r-1", "Spaghetti Carbonara"));
book.Add(new BakedRecipe("r-2", "Apple Crumble"));
book.Add(new NoCookRecipe("r-3", "Greek Salad"));

// 5. Demonstrate function passed as a parameter
Console.WriteLine("\n--- Function as Parameter (Filtering with Where) ---");
var quickRecipes = book.Matching("Salad");
foreach (var r in quickRecipes)
{
    Console.WriteLine($"Found: {r.DisplayName}");
}

// --- STRETCH TASK DEMO: Say WHAT happened (rich payload) ---
Console.WriteLine("\n--- Stretch Task: Plan Changed with Rich Payload ---");
var plan = new MealPlan();

plan.PlanChangedDetailed += change =>
{
    Console.WriteLine($"[Stretch Event] Action: {change.Action} | Recipe: '{change.RecipeName}' at {change.Timestamp:HH:mm:ss}");
};

plan.AddRecipe(new StovetopRecipe("r-1", "Spaghetti Carbonara"));
plan.AddRecipe(new BakedRecipe("r-2", "Apple Crumble"));
plan.RemoveRecipe("r-1");