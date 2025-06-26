using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Dto;
using SearchEngine.Tokenizer.Indexes;
using SearchEngine.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinFast : ExtendedSearchProcessorBase, IExtendedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndexHandler InvertedIndexExtended { get; init; }

    /// <inheritdoc/>
    public bool FindExtended(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var searchVector = processor.TokenizeText(text);

        if (searchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return false;
        }

        // Получить список с векторами из GIN на каждый токен вектора поискового запроса.
        var emptyDocIdVector = new DocIdVector([]);
        var docIdVectorSearchSpace = new List<DocIdVector>(searchVector.Count);
        foreach (var token in searchVector)
        {
            if (InvertedIndexExtended.TryGetIdentifiers(token, out var docIdVectorFromGin))
            {
                var docIdVectorCopy = docIdVectorFromGin.GetCopyInternal();
                foreach (var docIdVector in docIdVectorSearchSpace)
                {
                    docIdVectorCopy.ExceptWith(docIdVector);
                }

                docIdVectorSearchSpace.Add(docIdVectorCopy);
            }
            else
            {
                docIdVectorSearchSpace.Add(emptyDocIdVector);
            }
        }

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

        var directIndex = DirectIndexHandler.GetGeneralDirectIndex;
        for (var index = 0; index < docIdVectorSearchSpace.Count; index++)
        {
            var docIdVector = docIdVectorSearchSpace[index];
            foreach (var docId in docIdVector)
            {
                // direct: id -> tokens     | +positions            | direct extended - нужен только count для проверки границ
                // inverted: token -> ids   | +positions к docid    | к docid добавить размер в extended и reduced
                var targetVector = directIndex[docId].Extended;
                var comparisonScore = processor.ComputeComparisonScore(targetVector, searchVector, index);

                metricsCalculator.AppendExtended(comparisonScore, searchVector, docId, targetVector);
            }
        }

        return metricsCalculator.ContinueSearching;
    }
}

