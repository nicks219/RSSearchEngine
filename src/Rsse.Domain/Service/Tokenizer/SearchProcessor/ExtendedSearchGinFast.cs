using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
using SearchEngine.Service.Tokenizer.Indexes;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinFast : ExtendedSearchProcessorBase, IExtendedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler GinExtended { get; init; }

    /// <inheritdoc/>
    public bool FindExtended(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedSearchVector = processor.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return false;
        }

        var extendedDocIdVectorSearchSpace = TempStoragePool.VectorsTempStorage.Get();
        try
        {
            // extendedDocIdVectorSearchSpace.Clear();

            // todo: проверить отличие с установкой ёмкости коллекции в базовом классе
            extendedDocIdVectorSearchSpace.Capacity = TempStoragePool.StartTempStorageCapacity;
            CreateSearchSpaceFromVector(extendedSearchVector, extendedDocIdVectorSearchSpace);

            // поиск в векторе extended
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

            for (var index = 0; index < extendedDocIdVectorSearchSpace.Count; index++)
            {
                var docIdVector = extendedDocIdVectorSearchSpace[index];
                foreach (var docId in docIdVector)
                {
                    var tokensLine = GeneralDirectIndex[docId];
                    var extendedTokensLine = tokensLine.Extended;
                    var metric = processor.ComputeComparisonScore(extendedTokensLine, extendedSearchVector, index);

                    metricsCalculator.AppendExtended(metric, extendedSearchVector, docId, extendedTokensLine);
                }
            }

            return metricsCalculator.ContinueSearching;
        }
        finally
        {
            // Чистим коллекцию перед возвращением в пул.
            extendedDocIdVectorSearchSpace.Clear();
            TempStoragePool.VectorsTempStorage.Return(extendedDocIdVectorSearchSpace);
        }
    }

    /// <summary>
    /// Создать список с векторами из GIN ("пространство поиска") на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="extendedSearchVector">Вектор с поисковым запросом.</param>
    /// <param name="extendedDocIdVectorSearchSpace">Формируемое пространство поиска.</param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateSearchSpaceFromVector(
        TokenVector extendedSearchVector,
        List<DocIdVector> extendedDocIdVectorSearchSpace)
    {
        var emptyDocIdVector = new DocIdVector();

        foreach (var token in extendedSearchVector)
        {
            if (GinExtended.TryGetIdentifiers(token, out var docIdExtendedVector))
            {
                var docIdExtendedVectorCopy = docIdExtendedVector.GetCopyInternal();
                foreach (var docIdVector in extendedDocIdVectorSearchSpace)
                {
                    docIdExtendedVectorCopy.ExceptWith(docIdVector);
                }

                extendedDocIdVectorSearchSpace.Add(docIdExtendedVectorCopy);
            }
            else
            {
                extendedDocIdVectorSearchSpace.Add(emptyDocIdVector);
            }
        }
    }
}

