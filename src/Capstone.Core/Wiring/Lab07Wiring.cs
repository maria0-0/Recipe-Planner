using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab07Wiring : ILab07Wiring
{
    public IItemStore CreateStore() => new RecipeStore();

    public IItem CreateSample(int seed) =>
        new StovetopRecipe($"recipe-{seed}", $"Recipe Number {seed}");

    public Type ExtensionHost => typeof(RecipeQueries);

    public IEnumerable<string> Names(IItemStore store)
    {
        return ((RecipeStore)store).Select(i => i.DisplayName);
    }

    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items)
    {
        return items.SortedByName();
    }

    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take)
    {
        return items.Page(skip, take);
    }

    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n)
    {
        return items.Longest(n).ToList();
    }

    public StoreStats Summary(IEnumerable<IItem> items) => items.Summary();
}