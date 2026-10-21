using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab03Wiring : ILab03Wiring
{
    public IItemStore CreateStore() => new RecipeStore();

    public IItem CreateSample(int seed) =>
        new StovetopRecipe($"recipe-{seed}", $"Recipe Number {seed}");

    public IReadOnlyList<IItem> Search(IItemStore store, string query)
    {
        var matcher = SearchMatcher.Create();
        return store.All()
            .Where(item => matcher.Matches(item.DisplayName, query))
            .ToList();
    }
}
