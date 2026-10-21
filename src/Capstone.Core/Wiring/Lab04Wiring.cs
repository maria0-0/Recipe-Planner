using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab04Wiring : ILab04Wiring
{
    public IItemStore CreateStore() => new RecipeStore();

    public Type BaseType => typeof(Recipe);

    public IReadOnlyList<Type> KindTypes => [typeof(BakedRecipe), typeof(StovetopRecipe), typeof(NoCookRecipe)];

    public IItem CreateOfKind(int kindIndex, int seed) => kindIndex switch
    {
        0 => new BakedRecipe($"baked-{seed}", $"Baked Recipe {seed}", ovenTemperature: 180 + (seed % 40)),
        1 => new StovetopRecipe($"stovetop-{seed}", $"Stovetop Recipe {seed}", heatLevel: "Medium"),
        2 => new NoCookRecipe($"nocook-{seed}", $"No-Cook Recipe {seed}", requiresChilling: seed % 3 == 0),
        _ => throw new ArgumentOutOfRangeException(nameof(kindIndex), "Unknown kind index.")
    };

    public string Describe(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item is Recipe recipe ? recipe.Describe() : item.DisplayName;
    }

    public IReadOnlyList<string> DescribeAll(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var lines = new List<string>();
        foreach (var item in store.All())
        {
            lines.Add(Describe(item));
        }

        return lines;
    }

    public string Classify(IItem item) => item switch
    {
        null => throw new ArgumentNullException(nameof(item)),
        BakedRecipe => "baked",
        StovetopRecipe => "stovetop",
        NoCookRecipe => "no-cook",
        _ => "other"
    };

    public bool TryFindByName(IItemStore store, string name, out IItem? found)
    {
        if (store is null || string.IsNullOrWhiteSpace(name))
        {
            found = null;
            return false;
        }

        foreach (var item in store.All())
        {
            if (string.Equals(item.DisplayName, name, StringComparison.Ordinal))
            {
                found = item;
                return true;
            }
        }

        found = null;
        return false;
    }

    public int AddAll(IItemStore store, params IItem[] items)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (items is null) return 0;

        int count = 0;
        foreach (var item in items)
        {
            store.Add(item);
            count++;
        }

        return count;
    }
}
