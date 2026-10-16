using System.Collections;

namespace lab3.Domain;

/// <summary>
/// Обобщенный каталог с ограничением where T : class, IIdentifiable
/// </summary>
public class Catalog<T> : IEnumerable<T> where T : class, IIdentifiable
{
    private readonly List<T> _items = new();
    private readonly Dictionary<string, T> _byId = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _items.Count;

    public bool Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_byId.ContainsKey(item.Id))
            return false;

        _items.Add(item);
        _byId.Add(item.Id, item);
        return true;
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _items.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Индекс {index} вне диапазона [0..{_items.Count - 1}].");
            return _items[index];
        }
    }

    public T? this[string id]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _byId.GetValueOrDefault(id);
        }
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}