using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinMergeFilter : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex<DocumentIdList> GinReduced { get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReducedMerge(GinReduced, searchVector,
                    sortedIds, out int filteredTokensCount))
            {
                return;
            }

            switch (sortedIds.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        foreach (var documentId in sortedIds[0])
                        {
                            metricsCalculator.AppendReduced(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinMergeFilter));

                        using DocumentReducedScoreIterator documentReducedScoreIterator = new(TempStoragePool,
                            sortedIds, filteredTokensCount);

                        using MetricsConsumer metricsConsumer = new(TempStoragePool,
                            searchVector, metricsCalculator, GeneralDirectIndex, sortedIds, filteredTokensCount);

                        documentReducedScoreIterator.Iterate(metricsConsumer);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
        }
    }

    private readonly ref struct MetricsConsumer : DocumentReducedScoreIterator.IConsumer, IDisposable
    {
        private readonly TempStoragePool _tempStoragePool;
        private readonly TokenVector _searchVector;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly DirectIndex _generalDirectIndex;
        private readonly List<DocumentListEnumerator> _list;

        public MetricsConsumer(TempStoragePool tempStoragePool, TokenVector searchVector,
            IMetricsCalculator metricsCalculator, DirectIndex generalDirectIndex,
            List<DocumentIdList> sortedIds, int filteredTokensCount)
        {
            _tempStoragePool = tempStoragePool;
            _searchVector = searchVector;
            _metricsCalculator = metricsCalculator;
            _generalDirectIndex = generalDirectIndex;

            _list = _tempStoragePool.ListEnumeratorListsStorage.Get();

            for (var index = sortedIds.Count - 1; index >= filteredTokensCount; index--)
            {
                var docIdVector = sortedIds[index];
                _list.Add(docIdVector.CreateDocumentListEnumerator());
            }

            for (var index = 0; index < _list.Count; index++)
            {
                CollectionsMarshal.AsSpan(_list)[index].MoveNext();
            }
        }

        public void Dispose()
        {
            _tempStoragePool.ListEnumeratorListsStorage.Return(_list);
        }

        public void Accept(DocumentId documentId, int score)
        {
            var counter = 1;

            for (var index = _list.Count - 1; index >= 0; index--)
            {
                ref DocumentListEnumerator documentListEnumerator = ref CollectionsMarshal.AsSpan(_list)[index];

                if (documentListEnumerator.Current.Value < documentId.Value)
                {
                    if (documentListEnumerator.MoveNextBinarySearch(documentId))
                    {
                        if (documentListEnumerator.Current.Value < documentId.Value)
                        {
                            throw new InvalidOperationException();
                        }

                        if (documentListEnumerator.Current.Value == documentId.Value)
                        {
                            score++;

                            if (!documentListEnumerator.MoveNext())
                            {
                                _list.RemoveAt(index);
                            }
                        }
                    }
                    else
                    {
                        _list.RemoveAt(index);
                    }
                }
                else if (documentListEnumerator.Current.Value == documentId.Value)
                {
                    score++;

                    if (!documentListEnumerator.MoveNext())
                    {
                        _list.RemoveAt(index);
                    }
                }

                if (score <= counter)
                {
                    break;
                }

                counter++;
            }

            _metricsCalculator.AppendReduced(score, _searchVector, documentId, _generalDirectIndex);
        }
    }
}
