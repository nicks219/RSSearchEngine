using System;
using System.Threading;
using SimpleEngine.Algorithms.Legacy;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;
using SimpleEngine.SearchType;

namespace SimpleEngine.Selector;

/// <summary>
/// Компонент с алгоритмами для производственного окружения.
/// </summary>
public abstract class ProductionSearchAlgorithmSelector
{
    // Архитектурное ограничение.
    private ProductionSearchAlgorithmSelector() { }

    /// <summary>
    /// Legacy-алгоритм расширенного поиска.
    /// </summary>
    /// <param name="generalDirectIndexLegacy">Общий индекс идентификатор-вектор.</param>
    public sealed class ExtendedLegacy(GeneralDirectIndexLegacy generalDirectIndexLegacy) :
        ProductionSearchAlgorithmSelector,
        ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
    {
        /// <inheritdoc/>
        public void Find(ExtendedSearchType searchType, TokenVector searchVector,
            IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
        {
            switch (searchType)
            {
                case ExtendedSearchType.Legacy:
                    {
                        FindExtendedLegacy(searchVector, metricsCalculator, cancellationToken);
                        break;
                    }
                case ExtendedSearchType.SimpleLegacy:
                case ExtendedSearchType.DirectLinear:
                case ExtendedSearchType.DirectBinary:
                case ExtendedSearchType.DirectHash:
                default:
                    {
                        throw new NotSupportedException(
                            $"[{nameof(ExtendedSearchType)}] [{searchType.ToString()}] GIN optimization is not supported in production yet.");
                    }
            }
        }

        private void FindExtendedLegacy(TokenVector searchVector, IMetricsCalculator metricsCalculator,
            CancellationToken cancellationToken)
        {
            var extendedSearchLegacy = new ExtendedSearchLegacy
            {
                GeneralDirectIndexLegacy = generalDirectIndexLegacy
            };

            extendedSearchLegacy.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    /// <summary>
    /// Legacy-алгоритм сокращенного поиска.
    /// </summary>
    /// <param name="generalDirectIndexLegacy">Общий индекс идентификатор-вектор.</param>
    public sealed class ReducedLegacy(GeneralDirectIndexLegacy generalDirectIndexLegacy) :
        ProductionSearchAlgorithmSelector,
        ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    {
        /// <inheritdoc/>
        public void Find(ReducedSearchType searchType, TokenVector searchVector,
            IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
        {
            switch (searchType)
            {
                case ReducedSearchType.Legacy:
                    {
                        FindReducedLegacy(searchVector, metricsCalculator, cancellationToken);
                        break;
                    }
                case ReducedSearchType.SimpleLegacy:
                case ReducedSearchType.Direct:
                default:
                    {
                        throw new NotSupportedException(
                            $"[{nameof(ReducedSearchType)}] [{searchType.ToString()}] GIN optimization is not supported in production yet.");
                    }
            }
        }

        private void FindReducedLegacy(TokenVector searchVector, IMetricsCalculator metricsCalculator,
            CancellationToken cancellationToken)
        {
            var reducedSearchLegacy = new ReducedSearchLegacy
            {
                GeneralDirectIndexLegacy = generalDirectIndexLegacy
            };

            reducedSearchLegacy.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    // Необходимость пустого метода диктуется контрактом.
    public void AddVector(DocumentId documentId, TokenVector tokenVector) { }

    // Необходимость пустого метода диктуется контрактом.
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector) { }

    // Необходимость пустого метода диктуется контрактом.
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector) { }

    // Необходимость пустого метода диктуется контрактом.
    public void Clear() { }
}
