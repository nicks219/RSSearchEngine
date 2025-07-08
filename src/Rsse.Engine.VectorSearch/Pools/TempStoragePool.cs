using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RsseEngine.Contracts;
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
    internal readonly CollectionPool<Dictionary<DocumentId, int>, KeyValuePair<DocumentId, int>> ScoresStorage = new(enable);

    /// <summary>
    /// Пул для временного extended-пространства поиска.
    /// </summary>
    internal readonly CollectionPool<HashSet<DocumentId>, DocumentId> DocumentIdSetsStorage = new(enable);

    internal readonly CollectionPool<HashSet<Token>, Token> TokenSetsStorage = new(enable);

    private readonly CollectionPool<List<DocumentIdSet>, DocumentIdSet> _documentIdSetListsStorage = new(enable);

    private readonly CollectionPool<List<DocumentIdList>, DocumentIdList> _documentIdListListsStorage = new(enable);

    internal List<TDocumentIdCollection> GetDocumentIdCollectionList<TDocumentIdCollection>()
        where TDocumentIdCollection : struct, IDocumentIdCollection
    {
        if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            var documentIdCollection = _documentIdSetListsStorage.Get();
            return Unsafe.As<List<TDocumentIdCollection>>(documentIdCollection);
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            var documentIdCollection = _documentIdListListsStorage.Get();
            return Unsafe.As<List<TDocumentIdCollection>>(documentIdCollection);
        }

        throw new NotSupportedException($"[{nameof(TDocumentIdCollection)}] is not supported.");
    }

    internal void ReturnDocumentIdCollectionList<TDocumentIdCollection>(List<TDocumentIdCollection> idsFromGin)
        where TDocumentIdCollection : struct, IDocumentIdCollection
    {
        if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            var collections = Unsafe.As<List<DocumentIdSet>>(idsFromGin);
            _documentIdSetListsStorage.Return(collections);
            return;
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            var collections = Unsafe.As<List<DocumentIdList>>(idsFromGin);
            _documentIdListListsStorage.Return(collections);
            return;
        }

        throw new NotSupportedException($"[{nameof(TDocumentIdCollection)}] is not supported.");
    }

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
