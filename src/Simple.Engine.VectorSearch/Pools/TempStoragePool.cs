using System.Collections.Concurrent;
using System.Collections.Generic;
using SimpleEngine.Dto.Common;
using SimpleEngine.Iterators;

namespace SimpleEngine.Pools;

/// <summary>
/// Пул коллекций.
/// </summary>
/// <param name="enable">Пул активирован.</param>
public sealed class TempStoragePool(bool enable)
{
    internal readonly CollectionPool<List<DocumentIdsEnumerator>, DocumentIdsEnumerator> InternalEnumeratorCollections = new(enable);

    public readonly CollectionPool<List<int>, int> IntCollections = new(enable);

    internal readonly CollectionPool<Dictionary<InternalDocumentIds, int>, KeyValuePair<InternalDocumentIds, int>> InternalIdsStorage = new(enable);

    internal readonly CollectionPool<List<InternalDocumentIds>, InternalDocumentIds> InternalIdsCollections = new(enable);

    internal readonly CollectionPool<List<InternalDocumentIdsWithToken>, InternalDocumentIdsWithToken> InternalIdsWithTokenCollections = new(enable);

    // используется алгоритмами: DirectFilter - GinOffset - GinOffsetFilter
    internal readonly CollectionPool<List<DocumentIdsMergeEnumerator>, DocumentIdsMergeEnumerator> OffsetEnumeratorCollections = new(enable);

    /// <summary>
    /// Идентификаторы документов, прошедших порог релевантности.
    /// </summary>
    internal readonly CollectionPool<HashSet<DocumentId>, DocumentId> RelevantDocumentIds = new(enable);

    /// <summary>
    /// Счетчики совпавших с поисковым запросом токенов в документах.
    /// </summary>
    internal readonly CollectionPool<Dictionary<DocumentId, int>, KeyValuePair<DocumentId, int>> TokenOverlapCounts = new(enable);

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
