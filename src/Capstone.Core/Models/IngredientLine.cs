namespace Capstone.Core.Models;

/// <summary>
/// Represents one ingredient in a recipe with a quantity, unit, and optional cost/nutrition info.
/// </summary>
public record IngredientLine(
    string Name,
    decimal Quantity,
    Unit Unit,
    decimal? PricePerUnit = null,
    decimal? CaloriesPer100g = null);
