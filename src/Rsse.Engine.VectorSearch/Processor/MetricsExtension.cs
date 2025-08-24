using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Processor;

/// <summary>
/// Вычисление метрики сравнения двух векторов.
/// </summary>
public static class MetricsExtension
{
    public static void AppendExtendedMetric(this IMetricsCalculator metricsCalculator, int comparisonScore,
        TokenVector searchVector, ExternalDocumentIdWithSize externalDocument)
    {
        metricsCalculator.AppendExtended(comparisonScore, searchVector, externalDocument.ExternalDocumentId,
            externalDocument.Size);
    }

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

    public static void AppendReducedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, TokenLine tokenLine)
    {
        var reducedTargetVector = tokenLine.Reduced;
        var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

        // Для расчета метрик необходимо учитывать размер оригинальной заметки.
        metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector.Count);
    }

    public static void AppendReducedMetrics(this IMetricsCalculator metricsCalculator, DirectIndex directIndex,
        TokenVector searchVector, ComparisonScores comparisonScores)
    {
        foreach (var (documentId, score) in comparisonScores)
        {
            metricsCalculator.AppendReduced(score, searchVector, documentId, directIndex);
        }
    }
}
