using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Processor;

/// <summary>
/// Вычисление метрики сравнения двух векторов.
/// </summary>
public static class MetricsExtension
{
    public static void AppendExtendedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, DirectIndex directIndex, int searchStartIndex = 0)
    {
        var tokenLine = directIndex[documentId];

        AppendExtendedMetric(metricsCalculator, searchVector, documentId, tokenLine, searchStartIndex);
    }

    public static void AppendExtendedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, TokenLine tokenLine, int searchStartIndex = 0)
    {
        var extendedTargetVector = tokenLine.Extended;
        var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector, searchStartIndex);

        // Для расчета метрик необходимо учитывать размер оригинальной заметки.
        metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector);
    }

    public static void AppendReducedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, DirectIndex directIndex)
    {
        var tokenLine = directIndex[documentId];

        AppendReducedMetric(metricsCalculator, searchVector, documentId, tokenLine);
    }

    public static void AppendReducedMetric(this IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId, TokenLine tokenLine)
    {
        var reducedTargetVector = tokenLine.Reduced;
        var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

        // Для расчета метрик необходимо учитывать размер оригинальной заметки.
        metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector);
    }
}
