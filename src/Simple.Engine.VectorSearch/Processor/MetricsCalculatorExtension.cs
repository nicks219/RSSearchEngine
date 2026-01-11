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
        /// <summary>
        /// Добавить результат в виде метрики релевантности для расширенного поиска.
        /// Используется оптимизированными алгоритмами.
        /// </summary>
        /// <param name="comparisonScore">Баллы за совпавшие в обоих векторах токены.</param>
        /// <param name="searchVector">Вектор с поисковым запросом.</param>
        /// <param name="externalDocument">Контейнер с внешним идентификатором документа.</param>
        public void AppendExtendedMetric(
            int comparisonScore,
            TokenVector searchVector,
            ExternalDocumentIdWithSize externalDocument)
        {
            metricsCalculator.AppendExtended(
                comparisonScore,
                searchVector,
                externalDocument.ExternalDocumentId,
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
        public void AppendExtendedMetric(
            TokenVector searchVector,
            DocumentId documentId,
            TokenLine tokenLine,
            int searchStartIndex = 0)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = TokenOverlapScorer.CountOrderedMatches(extendedTargetVector, searchVector, searchStartIndex);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector.Count);
        }

        /// <summary>
        /// Добавить результат в виде метрики релевантности для нечеткого поиска.
        /// Используется оптимизированными алгоритмами.
        /// </summary>
        /// <param name="comparisonScore">Баллы за совпавшие в обоих векторах токены.</param>
        /// <param name="searchVector">Вектор с поисковым запросом.</param>
        /// <param name="externalDocument">Контейнер с внешним идентификатором документа.</param>
        public void AppendReducedMetric(
            int comparisonScore,
            TokenVector searchVector,
            ExternalDocumentIdWithSize externalDocument)
        {
            metricsCalculator.AppendReduced(
                comparisonScore,
                searchVector,
                externalDocument.ExternalDocumentId,
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

        /// <summary>
        /// Добавить результат в виде метрики релевантности для нечеткого поиска.
        /// Используется алгоритмом simple.
        /// </summary>
        /// <param name="searchVector">Вектор с поисковым запросом.</param>
        /// <param name="documentId">Идентификатор документа.</param>
        /// <param name="tokenLine">Контейнер с двумя векторами для документа.</param>
        /// <param name="comparisonScore">Количество совпавших токенов в обоих векторах.</param>
        public void AppendReducedMetric(
            TokenVector searchVector,
            DocumentId documentId,
            TokenLine tokenLine,
            int comparisonScore)
        {
            var reducedTargetVector = tokenLine.Reduced;

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector.Count);
        }
    }
}
