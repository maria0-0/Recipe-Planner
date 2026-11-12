using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core;


public static class RecipeQueries
{
   
    public static IEnumerable<IItem> Longest(this IEnumerable<IItem> items, int n)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return items
            .OrderByDescending(i => i.DisplayName.Length)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .Take(n);
    }

    public static IReadOnlyList<IItem> SortedByName(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<IItem> Page(this IEnumerable<IItem> items, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public static IEnumerable<Recipe> FridgeMatch(
        this IEnumerable<Recipe> recipes,
        IEnumerable<string> fridgeIngredients,
        int maxMissing = 1)
    {
        ArgumentNullException.ThrowIfNull(recipes);
        ArgumentNullException.ThrowIfNull(fridgeIngredients);

        var available = fridgeIngredients
            .Select(f => f.Trim().ToLowerInvariant())
            .ToHashSet();

        return recipes.Where(r =>
        {
            int missing = r.Ingredients.Count(i => !available.Contains(i.Name.Trim().ToLowerInvariant()));
            return missing <= maxMissing;
        });
    }

    public static IReadOnlyList<string> MissingIngredients(
        this Recipe recipe,
        IEnumerable<string> fridgeIngredients)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(fridgeIngredients);

        var available = fridgeIngredients
            .Select(f => f.Trim().ToLowerInvariant())
            .ToHashSet();

        return recipe.Ingredients
            .Where(i => !available.Contains(i.Name.Trim().ToLowerInvariant()))
            .Select(i => i.Name)
            .ToList();
    }

    public static IEnumerable<Recipe> Filter(
        this IEnumerable<Recipe> recipes,
        Diet? diet = null,
        int? maxMinutes = null,
        string? ingredient = null,
        string? cuisine = null)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        var query = recipes;

        if (diet.HasValue && diet.Value != Diet.None)
        {
            query = query.Where(r => r.DietTags.HasFlag(diet.Value));
        }

        if (maxMinutes.HasValue)
        {
            query = query.Where(r => r.TotalTimeMinutes <= maxMinutes.Value);
        }

        if (!string.IsNullOrWhiteSpace(ingredient))
        {
            query = query.Where(r => r.Ingredients.Any(i =>
                i.Name.Contains(ingredient, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(cuisine))
        {
            query = query.Where(r => string.Equals(r.Cuisine, cuisine, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }

    public static IReadOnlyList<(string Ingredient, int Count)> MostUsedIngredients(this IEnumerable<Recipe> recipes, int top = 5)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        return recipes
            .SelectMany(r => r.Ingredients)
            .GroupBy(i => i.Name.Trim().ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(top)
            .Select(g => (Ingredient: g.Key, Count: g.Count()))
            .ToList();
    }

    public static bool IsEmpty(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return !items.Any();
    }

    public static string OneLine(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return string.Join(", ", items.Select(i => i.DisplayName));
    }

    public static IReadOnlyList<(string Cuisine, double AverageMinutes)> AverageTimePerCuisine(this IEnumerable<Recipe> recipes)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        return recipes
            .GroupBy(r => r.Cuisine)
            .Select(g => (Cuisine: g.Key, AverageMinutes: g.Average(r => r.TotalTimeMinutes)))
            .ToList();
    }

    public static StoreStats Summary(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var list = items as IReadOnlyList<IItem> ?? items.ToList();

        if (list.Count == 0)
        {
            return new StoreStats(0, 0, null, 0);
        }

        int count = list.Count;
        int distinctInitials = list
            .Select(i => char.ToUpperInvariant(i.DisplayName[0]))
            .Distinct()
            .Count();

        string? longestName = list.Longest(1).FirstOrDefault()?.DisplayName;
        double averageLength = list.Average(i => i.DisplayName.Length);

        return new StoreStats(count, distinctInitials, longestName, averageLength);
    }
}