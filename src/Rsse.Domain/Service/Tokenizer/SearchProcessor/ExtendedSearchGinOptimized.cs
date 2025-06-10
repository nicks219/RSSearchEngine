using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public class ExtendedSearchGinOptimized : ExtendedSearchProcessorBase, IExtendedSearchProcessor
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

        // выбрать только те заметки, которые пригодны для extended поиска
        var tokenLinesExtendedSearchSpace = new Dictionary<DocId, TokenLine>();
        var emptyReducedVector = new TokenVector();
        foreach (var token in extendedSearchVector)
        {
            if (!GinExtended.TryGetIdentifiers(token, out var ids))
            {
                continue;
            }

            // если в gin есть токен, перебираем id заметок в которых он присутствует и формируем пространство поиска
            foreach (var docId in ids)
            {
                if (!tokenLinesExtendedSearchSpace.TryGetValue(docId, out var tokenLine))
                {
                    // сохраняется оригинальная поисковая метрика:
                    // var originalTokenLine = GeneralDirectIndex[docId];
                    // tokenLinesExtended[docId] = originalTokenLine with { Reduced = emptyReducedVector };

                    tokenLinesExtendedSearchSpace[docId] = new TokenLine(new TokenVector([token.Value]), emptyReducedVector);
                }
                else
                {
                    var extendedVector = tokenLine.Extended;
                    extendedVector.Add(token.Value);
                }
            }
        }

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinOptimized));
        foreach (var (docId, tokenLine) in tokenLinesExtendedSearchSpace)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, GeneralDirectIndex[docId].Extended);
        }

        return true;
    }
}
