using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

/// <summary>
/// Connects our recipe domain objects to the Lab 02 contract tests.
/// </summary>
public sealed class Lab02Wiring : ILab02Wiring
{
    public IItemStore CreateStore() => new RecipeStore();

    public IItemMapper CreateMapper() => new RecipeMapper();

    public IItem CreateSample(int seed) =>
        new Recipe($"recipe-{seed}", $"Recipe Number {seed}");
}
