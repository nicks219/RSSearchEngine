using System.Collections.Concurrent;
using System.Collections.Generic;
using RsseEngine.Dto.Common;
using RsseEngine.Iterators;

namespace RsseEngine.Pools;

/// <summary>
/// Пул коллекций.
/// </summary>
/// <param name="enable">Пул активирован.</param>
public sealed class TempStoragePool(bool enable)
{
    internal readonly CollectionPool<List<DocumentIdsEnumerator>, DocumentIdsEnumerator> InternalEnumeratorCollections = new(enable);

    public readonly CollectionPool<List<int>, int> IntCollections = new(enable);

    internal readonly CollectionPool<Dictionary<InternalDocumentIds, int>, KeyValuePair<InternalDocumentIds, int>> InternalIdsStorage = new(enable);

    internal readonly CollectionPool<List<DocumentIdsExtendedEnumerator>, DocumentIdsExtendedEnumerator> OffsetEnumeratorCollections = new(enable);

    internal readonly CollectionPool<List<InternalDocumentIds>, InternalDocumentIds> InternalIdsCollections = new(enable);

    internal readonly CollectionPool<List<InternalDocumentIdsWithToken>, InternalDocumentIdsWithToken> InternalIdsWithTokenCollections = new(enable);

    public sealed class CollectionPool<T1, T2> where T1 : ICollection<T2>, new()
    {
        private readonly ConcurrentBag<T1>? _bag;

        public CollectionPool(bool enable)
        {
            if (enable)
            {
                _bag = [];
            }
        }

        public T1 Get()
        {
            return _bag != null && _bag.TryTake(out var result)
                ? result
                : [];
        }

        public void Return(T1 result)
        {
            if (_bag != null)
            {
                result.Clear();
                _bag.Add(result);
            }
        }
    }
}
