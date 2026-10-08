namespace Capstone.Core.Models;

/// <summary>
/// Represents one ingredient in a recipe with a quantity and unit.
/// </summary>
public record IngredientLine(string Name, decimal Quantity, Unit Unit);
