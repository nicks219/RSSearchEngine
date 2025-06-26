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
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ExtendedSearchGinOptimized : ExtendedSearchProcessorBase, IExtendedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndexHandler InvertedIndexExtended { get; init; }

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

        // выбрать только те заметки, которые пригодны для extended поиска
        var idsExtendedSearchSpace = new HashSet<DocId>();
        foreach (var token in extendedSearchVector)
        {
            if (!InvertedIndexExtended.TryGetIdentifiers(token, out var docIds))
            {
                continue;
            }

            // если в gin есть токен, перебираем id заметок в которых он присутствует и формируем пространство поиска
            foreach (var docId in docIds)
            {
                // выигрывает по нагрузке на GC:
                // if (!tokenLinesExtendedSearchSpace.TryGetValue(docId, out var tokenLine)) {
                // var originalTokenLine = GeneralDirectIndex[docId];
                // tokenLinesExtendedSearchSpace[docId] = originalTokenLine with { Reduced = emptyReducedVector };

                idsExtendedSearchSpace.Add(docId);
            }
        }

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinOptimized));
        var directIndex = DirectIndexHandler.GetGeneralDirectIndex;
        foreach (var docId in idsExtendedSearchSpace)
        {
            var extendedTargetVector = directIndex[docId].Extended;
            var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, extendedTargetVector);
        }

        return true;
    }
}
