namespace Capstone.Core;


public class RecentlyUsed<T>
{
    private readonly List<T> _items = [];

    public int Capacity { get; }

    public int Count => _items.Count;

    public IReadOnlyList<T> Items => _items.AsReadOnly();

    public RecentlyUsed(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");

        Capacity = capacity;
    }

    public void Push(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        int existingIndex = _items.FindIndex(x => comparer.Equals(x, item));

        if (existingIndex >= 0)
        {
            _items.RemoveAt(existingIndex);
        }
        else if (_items.Count >= Capacity)
        {
            _items.RemoveAt(_items.Count - 1);
        }

        _items.Insert(0, item);
    }
}
