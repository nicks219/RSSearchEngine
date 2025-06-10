using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Processor;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public class ExtendedSearchGinOptimized : ExtendedMetricsBase, IExtendedMetricsProcessor
{
    /// <inheritdoc/>
    public bool FindExtended(string text, Dictionary<DocId, double> complianceMetrics, CancellationToken cancellationToken)
    {
        var continueSearching = true;

        var processor = ProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedSearchVector = processor.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return false;
        }

        // выбрать только те заметки, которые пригодны для extended поиска
        var tokenLinesExtended = new Dictionary<DocId, TokenLine>();
        var emptyReducedVector = new TokenVector();
        foreach (var token in extendedSearchVector)
        {
            // если в gin нет токена, то идём дальше
            if (!ExtendedGin.TryGetIdentifiers(token, out var ids))
            {
                continue;
            }

            // если в gin есть токен, перебираем id заметок в которых он присутствует
            foreach (var docId in ids)
            {
                if (!tokenLinesExtended.TryGetValue(docId, out var tokenLine))
                {
                    // сохраняется оригинальная поисковая метрика:
                    // var originalTokenLine = TokenLines[docId];
                    // tokenLinesExtended[docId] = originalTokenLine with { Reduced = emptyReducedVector };

                    // увеличивается оригинальная поисковая метрика, необходимо использовать оригинальный Extended.Count:
                    tokenLinesExtended[docId] = new TokenLine(new TokenVector([token.Value]), emptyReducedVector);
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
        foreach (var (docId, tokenLine) in tokenLinesExtended)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (comparisonScore == extendedSearchVector.Count)
            {
                continueSearching = false;
                complianceMetrics.Add(docId, comparisonScore * (1000D / extendedTargetVector.Count));
                continue;
            }

            // II. extended% совпадение
            if (comparisonScore >= extendedSearchVector.Count * ExtendedCoefficient)
            {
                // todo: можно так оценить
                // continueSearching = false;
                complianceMetrics.Add(docId, comparisonScore * (100D / extendedTargetVector.Count));
            }
        }

        return continueSearching;
    }
}
