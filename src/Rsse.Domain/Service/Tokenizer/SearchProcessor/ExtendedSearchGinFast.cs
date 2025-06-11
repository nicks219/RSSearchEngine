using System;
using System.Collections.Generic;
using System.Linq;
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

        var extendedGinVectorSearchSpaces = CreateExtendedSearchSpace(extendedSearchVector);

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

        for (var index = 0; index < extendedGinVectorSearchSpaces.Count; index++)
        {
            var ginVector = extendedGinVectorSearchSpaces[index];
            foreach (var docId in ginVector)
            {
                var tokensLine = GeneralDirectIndex[docId];
                var extendedTokensLine = tokensLine.Extended;
                var metric = processor.ComputeComparisonScore(extendedTokensLine, extendedSearchVector, index);

                metricsCalculator.AppendExtended(metric, extendedSearchVector, docId, extendedTokensLine);
            }
        }

        return metricsCalculator.ContinueSearching;
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="extendedSearchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
    private List<HashSet<DocId>> CreateExtendedSearchSpace(TokenVector extendedSearchVector)
    {
        var newGinVectors = new List<HashSet<DocId>>(extendedSearchVector.Count);

        foreach (var token in extendedSearchVector)
        {
            if (GinExtended.TryGetIdentifiers(token, out var ginVector))
            {
                var ginVectorCopy = ginVector.ToHashSet();
                foreach (var idVector in newGinVectors)
                {
                    ginVectorCopy.ExceptWith(idVector);
                }

                newGinVectors.Add(ginVectorCopy);
            }
            else
            {
                newGinVectors.Add([]);
            }
        }

        return newGinVectors;
    }
}

/* метод:
private List<DocIdVector> CreateDocIdVectorEx(TokenVector newTokensLine)
    {
        List<DocIdVector> docIdVector = new List<DocIdVector>(newTokensLine.Count);
        //HashSet<DocId> hashSet = new HashSet<DocId>();

        foreach (var hash in newTokensLine)
        {
            if (_invertedIndexEx.TryGetValue(hash, out var set))
            {
                var asdf = set.Value.ToHashSet();

                foreach (var idVector in docIdVector)
                {
                    asdf.ExceptWith(idVector.Value);
                }
                docIdVector.Add(new DocIdVector(asdf));
            }
            else
            {
                docIdVector.Add(new DocIdVector(new HashSet<DocId>(0)));
            }
        }

        return docIdVector;
    }
    */

/* использование:
 List<DocIdVector> exHashId = CreateDocIdVectorEx(newTokensLine);

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ComputeComplianceIndices));

        for (int index = 0; index < exHashId.Count; index++)
        {
            foreach (DocId docId in exHashId[index])
            {
                var tokensLine = _generalDirectIndex[docId];
                var extendedTokensLine = tokensLine.Extended;
                var metric = processor.ComputeComparisonScore(extendedTokensLine, newTokensLine, index);

                metricsCalculator.AppendExtended(metric, newTokensLine, docId, extendedTokensLine);
            }
        }

        if (!metricsCalculator.ContinueSearching)
        {
            return metricsCalculator.ComplianceMetrics;
        }
 */

// старый вариант:
// выбрать только те заметки, которые пригодны для extended поиска
/*var idsExtendedSearchSpace = new HashSet<DocId>();
foreach (var token in extendedSearchVector)
{
    if (!GinExtended.TryGetIdentifiers(token, out var docIds))
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
foreach (var docId in idsExtendedSearchSpace)
{
    var extendedTargetVector = GeneralDirectIndex[docId].Extended;
    var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

    // Для расчета метрик необходимо учитывать размер оригинальной заметки.
    metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, extendedTargetVector);
}

return true;*/
