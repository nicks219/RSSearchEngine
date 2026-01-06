using System.Collections.Concurrent;
using System.Collections.Generic;
using RsseEngine.Dto.Common;
using RsseEngine.Iterators;

namespace RsseEngine.Pools;

/// <summary>
/// Пул коллекций
/// </summary>
/// <param name="enable">Пул активирован.</param>
public sealed class TempStoragePool(bool enable)
{
    internal readonly CollectionPool<List<InternalDocumentListEnumerator>, InternalDocumentListEnumerator> ListInternalEnumeratorListsStorage = new(enable);

    public readonly CollectionPool<List<int>, int> IntListsStorage = new(enable);

    internal readonly CollectionPool<Dictionary<InternalDocumentIds, int>, KeyValuePair<InternalDocumentIds, int>> InternalDocumentIdListCountStorage = new(enable);

    internal readonly CollectionPool<List<TokenOffsetEnumerator>, TokenOffsetEnumerator> TokenOffsetEnumeratorListsStorage = new(enable);

    internal readonly CollectionPool<List<InternalDocumentIds>, InternalDocumentIds> InternalDocumentIdListsStorage = new(enable);

    internal readonly CollectionPool<List<InternalDocumentIdsWithToken>, InternalDocumentIdsWithToken> InternalDocumentIdListsWithTokenStorage = new(enable);

    public sealed class CollectionPool<T, TV> where T : ICollection<TV>, new()
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
