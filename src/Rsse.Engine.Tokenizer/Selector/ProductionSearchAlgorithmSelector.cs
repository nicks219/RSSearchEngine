using System;
using System.Threading;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.SearchType;

namespace RsseEngine.Selector;

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
    /// <param name="generalDirectIndex">Общий индекс идентификатор-вектор.</param>
    public sealed class ExtendedLegacy(DirectIndex generalDirectIndex) :
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
                default:
                    {
                        throw new NotSupportedException(
                            $"Extended[{searchType.ToString()}] GIN optimization is not supported in production yet.");
                    }
            }
        }

        private void FindExtendedLegacy(TokenVector searchVector, IMetricsCalculator metricsCalculator,
            CancellationToken cancellationToken)
        {
            var extendedSearchLegacy = new ExtendedSearchLegacy
            {
                GeneralDirectIndex = generalDirectIndex
            };

            extendedSearchLegacy.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    /// <summary>
    /// Legacy-алгоритм сокращенного поиска.
    /// </summary>
    /// <param name="generalDirectIndex">Общий индекс идентификатор-вектор.</param>
    public sealed class ReducedLegacy(DirectIndex generalDirectIndex) :
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
                default:
                    {
                        throw new NotSupportedException(
                            $"Reduced[{searchType.ToString()}] GIN optimization is not supported in production yet.");
                    }
            }
        }

        private void FindReducedLegacy(TokenVector searchVector, IMetricsCalculator metricsCalculator,
            CancellationToken cancellationToken)
        {
            var reducedSearchLegacy = new ReducedSearchLegacy
            {
                GeneralDirectIndex = generalDirectIndex
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
