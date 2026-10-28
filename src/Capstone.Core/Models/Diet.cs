namespace Capstone.Core.Models;

/// <summary>
/// Dietary preferences and allergy tags that can be combined with bitwise flags.
/// </summary>
[Flags]
public enum Diet
{
    None = 0,
    Vegetarian = 1 << 0,
    Vegan = 1 << 1,
    GlutenFree = 1 << 2,
    DairyFree = 1 << 3,
    NutFree = 1 << 4,
    Keto = 1 << 5,
    LowCarb = 1 << 6
}
