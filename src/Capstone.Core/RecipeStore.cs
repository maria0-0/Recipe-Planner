using System.Collections;
using SharpQuest.Contracts;

namespace Capstone.Core;

/// <summary>
/// In-memory storage and collection for recipes and items (The Recipe Book).
/// </summary>
public class RecipeStore : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> _items = [];

    public int Count => _items.Count;

    /// <summary>
    /// Indexer by position in the order items were added.
    /// Throws ArgumentOutOfRangeException if index is outside the collection.
    /// </summary>
    public IItem this[int index] => _items[index];

    /// <summary>
    /// Indexer by recipe ID. Returns the item or null if not found.
    /// </summary>
    public IItem? this[string id] => FindById(id);

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(i => string.Equals(i.Id, item.Id, StringComparison.Ordinal)))
            throw new ArgumentException($"An item with Id '{item.Id}' already exists.", nameof(item));

        _items.Add(item);
    }

    public IItem? FindById(string id)
    {
        if (id is null) return null;

        return _items.Find(i => string.Equals(i.Id, id, StringComparison.Ordinal));
    }

    public IReadOnlyList<IItem> All()
    {
        // Returns a new copy (snapshot) so outside modifications don't affect this list
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

    /// <summary>
    /// Iterates through the store in the order items were added.
    /// Written with yield return as an iterator.
    /// </summary>
    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns recipes matching the given text in DisplayName.
    /// Written with yield return so it is deferred (lazy) until enumerated.
    /// </summary>
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
}
