using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RD.RsseEngine.Tokenizer.Common;

/// <summary>
/// Пул коллекций, использующий Concurrent Bag.
/// </summary>
/// <typeparam name="TCollection">Коллекция элементов типа int.</typeparam>
public sealed class ConcurrentBagPool<TCollection> where TCollection : ICollection<int>, new()
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
