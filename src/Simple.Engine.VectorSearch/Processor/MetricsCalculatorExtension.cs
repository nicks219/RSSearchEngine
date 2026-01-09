using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Processor;

/// <summary>
/// Расширения для функционала подсчёта релевантности документов поисковому запросу.
/// </summary>
public static class MetricsCalculatorExtension
{
    /// <param name="metricsCalculator">Функционал подсчёта метрик релевантности.</param>
    extension(IMetricsCalculator metricsCalculator)
    {
        public void AppendExtendedMetric(int comparisonScore,
            TokenVector searchVector, ExternalDocumentIdWithSize externalDocument)
        {
            metricsCalculator.AppendExtended(comparisonScore, searchVector, externalDocument.ExternalDocumentId,
                externalDocument.Size);
        }

        /// <summary>
        /// Добавить результат в виде метрики релевантности для расширенного поиска.
        /// Используется алгоритмом legacy.
        /// </summary>
        /// <param name="searchVector">Вектор с поисковым запросом.</param>
        /// <param name="documentId">Идентификатор документа.</param>
        /// <param name="tokenLine">Контейнер с двумя векторами для документа.</param>
        /// <param name="searchStartIndex"></param>
        public void AppendExtendedMetric(TokenVector searchVector, DocumentId documentId, TokenLine tokenLine, int searchStartIndex = 0)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = TokenOverlapScorer.CountOrderedMatches(extendedTargetVector, searchVector, searchStartIndex);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector.Count);
        }

        public void AppendReducedMetric(int comparisonScore, TokenVector searchVector, ExternalDocumentIdWithSize externalDocument)
        {
            metricsCalculator.AppendReduced(comparisonScore, searchVector, externalDocument.ExternalDocumentId,
                externalDocument.Size);
        }

        /// <summary>
        /// Добавить результат в виде метрики релевантности для нечеткого поиска.
        /// Используется алгоритмом legacy.
        /// </summary>
        /// <param name="searchVector">Вектор с поисковым запросом.</param>
        /// <param name="documentId">Идентификатор документа.</param>
        /// <param name="tokenLine">Контейнер с двумя векторами для документа.</param>
        public void AppendReducedMetric(TokenVector searchVector, DocumentId documentId, TokenLine tokenLine)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = TokenOverlapScorer.CountUnorderedMatches(reducedTargetVector, searchVector);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector.Count);
        }
    }
}
