using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RsseEngine.Tokenizer.Common;

/// <summary>
/// Пул коллекций, использующий Concurrent Bag.
/// </summary>
/// <typeparam name="TCollection">Коллекция элементов типа T.</typeparam>
/// <typeparam name="TElement">Коллекция элементов типа T.</typeparam>
public sealed class ConcurrentBagPool<TCollection, TElement> where TCollection : ICollection<TElement>, new()
{
    private readonly ConcurrentBag<TCollection> _bag = [];

    /// <summary>
    /// Получить коллекцию из пула.
    /// </summary>
    /// <returns>Коллекция - из пула, либо заново созданная.</returns>
    public TCollection Get()
    {
        var collection = _bag.TryTake(out var result)
            ? result
            : [];

        return collection;
    }

    /// <summary>
    /// Вернуть коллекцию в пул.
    /// </summary>
    /// <param name="collection">Коллекция, возвращаемая в пул.</param>
    public void Return(TCollection collection)
    {
        collection.Clear();
        _bag.Add(collection);
    }
}
