using System.Collections.Concurrent;
using System.Collections.Generic;
using RsseEngine.Dto;

namespace RsseEngine.Pools;

/// <summary>
/// Пул коллекций
/// </summary>
/// <param name="enable">Пул активирован.</param>
public sealed class TempStoragePool(bool enable)
{
    /// <summary>
    /// Пул для временных reduced-метрик.
    /// </summary>
    internal readonly CollectionPool<Dictionary<DocumentId, int>, KeyValuePair<DocumentId, int>> ScoresTempStorage = new(enable);

    /// <summary>
    /// Пул для временного extended-пространства поиска.
    /// </summary>
    internal readonly CollectionPool<HashSet<DocumentId>, DocumentId> SetsTempStorage = new(enable);

    internal sealed class CollectionPool<T, TV> where T : ICollection<TV>, new()
    {
        private readonly ConcurrentBag<T>? _bag;

        public CollectionPool(bool enable)
        {
            if (enable)
            {
                _bag = [];
            }
        }

        public T Get()
        {
            return _bag != null && _bag.TryTake(out var result)
                ? result
                : [];
        }

        public void Return(T result)
        {
            if (_bag != null)
            {
                result.Clear();
                _bag.Add(result);
            }
        }
    }
}
