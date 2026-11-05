using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab06Wiring : ILab06Wiring
{
    public IItemStore CreateStore() => new RecipeStore();

    public IItem CreateSample(int seed) =>
        new StovetopRecipe($"recipe-{seed}", $"Recipe Number {seed}");

    public Type PublisherType => typeof(RecipeStore);

    public Action Subscribe(IItemStore store, Action<IItem> handler)
    {
        var recipeStore = (RecipeStore)store;
        recipeStore.ItemAdded += handler;
        return () => recipeStore.ItemAdded -= handler;
    }

    public IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep)
    {
        ArgumentNullException.ThrowIfNull(keep);

        var recipeStore = (RecipeStore)store;
        var result = new List<IItem>();
        foreach (var item in recipeStore)
        {
            if (keep(item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    public IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> describe)
    {
        ArgumentNullException.ThrowIfNull(describe);

        var recipeStore = (RecipeStore)store;
        var result = new List<string>();
        foreach (var item in recipeStore)
        {
            result.Add(describe(item));
        }
        return result;
    }

    public void ForEach(IItemStore store, Action<IItem> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var recipeStore = (RecipeStore)store;
        foreach (var item in recipeStore)
        {
            action(item);
        }
    }
}