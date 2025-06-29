using System;
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
        // Legacy-алгоритм без GIN-индекса.
        private readonly ExtendedSearchLegacy _extendedSearchLegacy = new()
        {
            GeneralDirectIndex = generalDirectIndex
        };

        /// <inheritdoc/>
        public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
        {
            return searchType switch
            {
                ExtendedSearchType.Legacy => _extendedSearchLegacy,
                _ => throw new NotSupportedException(
                    $"Extended[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
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
        // Legacy-алгоритм без GIN-индекса.
        private readonly ReducedSearchLegacy _reducedSearchLegacy = new()
        {
            GeneralDirectIndex = generalDirectIndex
        };

        /// <inheritdoc/>
        public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
        {
            return searchType switch
            {
                ReducedSearchType.Legacy => _reducedSearchLegacy,
                _ => throw new NotSupportedException(
                    $"Reduced[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }
    }

    // Необходимость пустого метода диктуется контрактом.
    public void AddVector(DocumentId documentId, TokenVector tokenVector) { }

    // Необходимость пустого метода диктуется контрактом.
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector) { }

    // Необходимость пустого метода диктуется контрактом.
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector) { }

    // Необходимость пустого метода диктуется контрактом.
    public void Clear() { }
}
