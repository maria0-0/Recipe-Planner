namespace Capstone.Core.Models;

/// <summary>
/// A category that groups related recipes (e.g. Pasta, Dessert, Breakfast).
/// </summary>
public class Category
{
    public string Name { get; }
    public string Description { get; }
    private readonly List<Recipe> _recipes = [];

    public IReadOnlyList<Recipe> Recipes => _recipes;

    public Category(string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        Name = name;
        Description = description;
    }

    public void AddRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        _recipes.Add(recipe);
    }
}
