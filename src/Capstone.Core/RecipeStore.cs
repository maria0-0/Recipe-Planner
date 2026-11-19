using System.Collections;
using SharpQuest.Contracts;
using Capstone.Core.Models;

namespace Capstone.Core;

//In-memory storage and collection for recipes and items (The Recipe Book).

public class RecipeStore : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> _items = [];

    public int Count => _items.Count;

    public event Action<IItem>? ItemAdded;

    public event Action<Recipe>? FavoriteToggled;

    public event Action<Recipe>? CookingLogged;
  
    public IItem this[int index] => _items[index];

   
    public IItem? this[string id] => FindById(id);

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(i => string.Equals(i.Id, item.Id, StringComparison.Ordinal)))
            throw new ArgumentException($"An item with Id '{item.Id}' already exists.", nameof(item));

        _items.Add(item);

        ItemAdded?.Invoke(item);
    }

    public IItem? FindById(string id)
    {
        if (id is null) return null;

        return _items.Find(i => string.Equals(i.Id, id, StringComparison.Ordinal));
    }

    public IReadOnlyList<IItem> All()
    {
        return _items.ToList();
    }

    public bool Remove(string id)
    {
        if (id is null) return false;

        var index = _items.FindIndex(i => string.Equals(i.Id, id, StringComparison.Ordinal));
        if (index < 0) return false;

        _items.RemoveAt(index);
        return true;
    }


    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  
    public IEnumerable<IItem> Matching(string text)
    {
        foreach (var item in _items)
        {
            if (string.IsNullOrEmpty(text) || item.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                yield return item;
            }
        }
    }

    private readonly List<CookingHistoryEntry> _cookingHistory = [];

    public IReadOnlyList<CookingHistoryEntry> CookingHistory => _cookingHistory;

    public IReadOnlyList<Recipe> Favorites => _items.OfType<Recipe>().Where(r => r.IsFavorite).ToList();

    public void ToggleFavorite(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        recipe.IsFavorite = !recipe.IsFavorite;
        FavoriteToggled?.Invoke(recipe);
    }

    public void LogCooking(Recipe recipe, DateTime? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        _cookingHistory.Add(new CookingHistoryEntry(recipe.Id, recipe.DisplayName, timestamp ?? DateTime.Now));
        CookingLogged?.Invoke(recipe);
    }
}
