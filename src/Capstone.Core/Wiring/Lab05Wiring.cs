using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab05Wiring : ILab05Wiring
{
    public IItemStore CreateStore() => new RecipeStore();

    public IItem CreateSample(int seed) =>
        new StovetopRecipe($"recipe-{seed}", $"Recipe Number {seed}");

    public Type CollectionType => typeof(RecipeStore);

    public Type GenericDefinition => typeof(RecentlyUsed<>);

    public IItem At(IItemStore store, int index)
    {
        if (store is RecipeStore rs)
            return rs[index];

        throw new ArgumentException("Store is not a RecipeStore.", nameof(store));
    }

    public IItem? ById(IItemStore store, string id)
    {
        if (store is RecipeStore rs)
            return rs[id];

        return store.FindById(id);
    }

    public IReadOnlyList<IItem> EachInOrder(IItemStore store)
    {
        var list = new List<IItem>();
        if (store is IEnumerable<IItem> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
        return list;
    }

    public IEnumerable<IItem> Matching(IItemStore store, string text)
    {
        if (store is RecipeStore rs)
            return rs.Matching(text);

        return [];
    }

    public IReadOnlyList<T> Recent<T>(int capacity, params T[] pushed)
    {
        var tracker = new RecentlyUsed<T>(capacity);
        if (pushed is not null)
        {
            foreach (var value in pushed)
            {
                tracker.Push(value);
            }
        }
        return tracker.Items;
    }
}
