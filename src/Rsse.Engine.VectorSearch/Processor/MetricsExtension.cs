using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Processor;

/// <summary>
/// Вычисление метрики сравнения двух векторов для формирования результатов поиска.
/// </summary>
public static class MetricsExtension
{
    public static void AppendExtendedMetric(this IMetricsCalculator metricsCalculator, int comparisonScore,
        TokenVector searchVector, ExternalDocumentIdWithSize externalDocument)
    {
        metricsCalculator.AppendExtended(comparisonScore, searchVector, externalDocument.ExternalDocumentId,
            externalDocument.Size);
    }

    /// <summary>
    /// Добавить результат в виде метрики релевантности для расширенного поиска.
    /// Используется алгоритмом legacy.
    /// </summary>
    /// <param name="metricsCalculator">Функционал подсчёта метрик релевантности.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenLine">Контейнер с двумя векторами для документа.</param>
    /// <param name="searchStartIndex"></param>
    public static void AppendExtendedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, TokenLine tokenLine, int searchStartIndex = 0)
    {
        var extendedTargetVector = tokenLine.Extended;
        var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector, searchStartIndex);

        // Для расчета метрик необходимо учитывать размер оригинальной заметки.
        metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector.Count);
    }

    public static void AppendReducedMetric(this IMetricsCalculator metricsCalculator, int comparisonScore,
        TokenVector searchVector, ExternalDocumentIdWithSize externalDocument)
    {
        metricsCalculator.AppendReduced(comparisonScore, searchVector, externalDocument.ExternalDocumentId,
            externalDocument.Size);
    }

    /// <summary>
    /// Добавить результат в виде метрики релевантности для нечеткого поиска.
    /// Используется алгоритмом legacy.
    /// </summary>
    /// <param name="metricsCalculator">Функционал подсчёта метрик релевантности.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenLine">Контейнер с двумя векторами для документа.</param>
    public static void AppendReducedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, TokenLine tokenLine)
    {
        var reducedTargetVector = tokenLine.Reduced;
        var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

        // Для расчета метрик необходимо учитывать размер оригинальной заметки.
        metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector.Count);
    }

    public static void AppendReducedMetricsFromSingleIndex(this IMetricsCalculator metricsCalculator,
        TokenVector searchVector, InvertedIndex invertedIndex, InternalDocumentIdList documentIds)
    {
        foreach (var documentId in documentIds)
        {
            if (invertedIndex.TryGetOffsetTokenVector(documentId, out _, out var externalDocument))
            {
                const int metric = 1;
                AppendReducedMetric(metricsCalculator, metric, searchVector, externalDocument);
            }
        }
    }

    public static void AppendExtendedMetricsFromSingleIndex(this IMetricsCalculator metricsCalculator,
        TokenVector searchVector, InvertedIndex invertedIndex, InternalDocumentIdList documentIds)
    {
        foreach (var documentId in documentIds)
        {
            if (invertedIndex.TryGetOffsetTokenVector(documentId, out _, out var externalDocument))
            {
                const int metric = 1;
                AppendExtendedMetric(metricsCalculator, metric, searchVector, externalDocument);
            }
        }
    }
}
