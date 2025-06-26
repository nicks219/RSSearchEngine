using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using SearchEngine.Service.Tokenizer.Contracts;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Функционал, поставляющий различные алгоритмы вычисления метрик поиска.
/// Для бенчмарков.
/// </summary>
public sealed class SearchProcessorFactory
{
    private readonly ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor> _extendedSearchAlgorithmSelector;

    private readonly ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor> _reducedSearchAlgorithmSelector;

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    private readonly DirectIndex _generalDirectIndex = new();

    /// <summary>
    /// Инициализация требуемого типа алгоритма.
    /// </summary>
    /// <exception cref="ArgumentNullException">Отсутствует контейнер с GIN при его требовании в оптимизации.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип оптимизации.</exception>
    public SearchProcessorFactory()
    {
        if (CheckIsProduction())
        {
            _extendedSearchAlgorithmSelector = new ProductionExtendedSearchAlgorithmSelector(_generalDirectIndex);
            _reducedSearchAlgorithmSelector = new ProductionReducedSearchAlgorithmSelector(_generalDirectIndex);
        }
        else
        {
            _extendedSearchAlgorithmSelector = new ExtendedSearchAlgorithmSelector(_generalDirectIndex);
            _reducedSearchAlgorithmSelector = new ReducedSearchAlgorithmSelector(_generalDirectIndex);
        }
    }

    public int Count => _generalDirectIndex.Count;

    public DirectIndex GetTokenLines()
    {
        return _generalDirectIndex;
    }

    public bool TryAdd(DocumentId documentId, TokenLine tokenLine)
    {
        if (!_generalDirectIndex.TryAdd(documentId, tokenLine))
        {
            return false;
        }

        _extendedSearchAlgorithmSelector.AddVector(documentId, tokenLine.Extended);
        _reducedSearchAlgorithmSelector.AddVector(documentId, tokenLine.Reduced);

        return true;
    }

    public bool TryUpdate(DocumentId documentId, TokenLine tokenLine)
    {
        if (!_generalDirectIndex.TryUpdate(documentId, tokenLine, out var oldTokenLine))
        {
            return false;
        }

        _extendedSearchAlgorithmSelector.UpdateVector(documentId, tokenLine.Extended, oldTokenLine.Extended);
        _reducedSearchAlgorithmSelector.UpdateVector(documentId, tokenLine.Reduced, oldTokenLine.Reduced);

        return true;
    }

    public bool TryRemove(DocumentId documentId)
    {
        if (!_generalDirectIndex.TryRemove(documentId, out var tokenLine))
        {
            return false;
        }

        _extendedSearchAlgorithmSelector.RemoveVector(documentId, tokenLine.Extended);
        _reducedSearchAlgorithmSelector.RemoveVector(documentId, tokenLine.Reduced);

        return true;
    }

    public void Clear()
    {
        _generalDirectIndex.Clear();
        _extendedSearchAlgorithmSelector.Clear();
        _reducedSearchAlgorithmSelector.Clear();
    }

    /// <summary>
    /// Алгоритм поиска текста в extended-векторах и подсчёта расширенной метрики.
    /// </summary>
    /// <param name="extendedSearchType">Тип оптимизации алгоритма поиска.</param>
    /// <param name="extendedSearchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="NotSupportedException"></exception>
    public void FindExtended(ExtendedSearchType extendedSearchType, TokenVector extendedSearchVector,
        MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        _extendedSearchAlgorithmSelector.GetSearchProcessor(extendedSearchType)
            .FindExtended(extendedSearchVector, metricsCalculator, cancellationToken);
    }

    /// <summary>
    /// Алгоритм поиска текста в reduced-векторах и подсчёта сокращенной метрики.
    /// </summary>
    /// <param name="reducedSearchType">Тип оптимизации алгоритма поиска.</param>
    /// <param name="reducedSearchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="NotSupportedException"></exception>
    public void FindReduced(ReducedSearchType reducedSearchType, TokenVector reducedSearchVector,
        MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        _reducedSearchAlgorithmSelector.GetSearchProcessor(reducedSearchType)
            .FindReduced(reducedSearchVector, metricsCalculator, cancellationToken);
    }

    /// <summary>
    /// Упасть при запуске в производственном окружении.
    /// </summary>
    private static bool CheckIsProduction()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "production";
    }

    private interface ISearchAlgorithmSelector<in TSearchType, out TSearchProcessor>
        where TSearchType : Enum
    {
        TSearchProcessor GetSearchProcessor(TSearchType searchType);

        void AddVector(DocumentId documentId, TokenVector tokenVector);

        void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector);

        void RemoveVector(DocumentId documentId, TokenVector tokenVector);

        void Clear();
    }

    private sealed class ProductionExtendedSearchAlgorithmSelector
        : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
    {
        private readonly ExtendedSearch _extendedSearch;

        public ProductionExtendedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
        {
            // Без GIN-индекса.
            _extendedSearch = new ExtendedSearch
            {
                GeneralDirectIndex = generalDirectIndex
            };
        }

        public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
        {
            return searchType switch
            {
                ExtendedSearchType.Original => _extendedSearch,
                _ => throw new NotSupportedException(
                    $"Extended[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }

        public void AddVector(DocumentId documentId, TokenVector tokenVector)
        {
            // Do nothing
        }

        public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
        {
            // Do nothing
        }

        public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
        {
            // Do nothing
        }

        public void Clear()
        {
            // Do nothing
        }
    }

    private sealed class ExtendedSearchAlgorithmSelector
        : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
    {
        /// <summary>
        /// Поддержка GIN-индекса для расширенного поиска и метрик.
        /// </summary>
        private readonly GinHandler<DocumentIdSet> _ginExtended = new();
        private readonly ExtendedSearch _extendedSearch;
        private readonly ExtendedSearchGin _extendedSearchGin;
        private readonly ExtendedSearchGinOptimized _extendedSearchGinOptimized;
        private readonly ExtendedSearchGinFast _extendedSearchGinFast;

        public ExtendedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
        {
            // Без GIN-индекса.
            _extendedSearch = new ExtendedSearch
            {
                GeneralDirectIndex = generalDirectIndex
            };

            // С GIN-индексом.
            _extendedSearchGin = new ExtendedSearchGin
            {
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = _ginExtended
            };

            // С GIN-индексом.
            _extendedSearchGinOptimized = new ExtendedSearchGinOptimized
            {
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = _ginExtended
            };

            // С GIN-индексом.
            _extendedSearchGinFast = new ExtendedSearchGinFast
            {
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = _ginExtended
            };
        }

        public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
        {
            return searchType switch
            {
                ExtendedSearchType.Original => _extendedSearch,
                ExtendedSearchType.GinSimple => _extendedSearchGin,
                ExtendedSearchType.GinOptimized => _extendedSearchGinOptimized,
                ExtendedSearchType.GinFast => _extendedSearchGinFast,
                _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                    "unknown search type")
            };
        }

        public void AddVector(DocumentId documentId, TokenVector tokenVector)
        {
            _ginExtended.AddVector(documentId, tokenVector);
        }

        public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
        {
            _ginExtended.UpdateVector(documentId, tokenVector, oldTokenVector);
        }

        public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
        {
            _ginExtended.RemoveVector(documentId, tokenVector);
        }

        public void Clear()
        {
            _ginExtended.Clear();
        }
    }

    private sealed class ProductionReducedSearchAlgorithmSelector
        : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    {
        private readonly ReducedSearch _reducedSearch;

        public ProductionReducedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
        {
            // Без GIN-индекса.
            _reducedSearch = new ReducedSearch
            {
                GeneralDirectIndex = generalDirectIndex
            };
        }

        public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
        {
            return searchType switch
            {
                ReducedSearchType.Original => _reducedSearch,
                _ => throw new NotSupportedException(
                    $"Reduced[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }

        public void AddVector(DocumentId documentId, TokenVector tokenVector)
        {
            // Do nothing
        }

        public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
        {
            // Do nothing
        }

        public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
        {
            // Do nothing
        }

        public void Clear()
        {
            // Do nothing
        }
    }

    private sealed class ReducedSearchAlgorithmSelector
        : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    {
        /// <summary>
        /// Поддержка GIN-индекса для сокращенного поиска и метрик.
        /// </summary>
        private readonly GinHandler<DocumentIdSet> _ginReduced = new();
        private readonly ReducedSearch _reducedSearch;
        private readonly ReducedSearchGin _reducedSearchGin;
        private readonly ReducedSearchGinOptimized _reducedSearchGinOptimized;
        private readonly ReducedSearchGinFast _reducedSearchGinFast;

        public ReducedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
        {
            // Без GIN-индекса.
            _reducedSearch = new ReducedSearch
            {
                GeneralDirectIndex = generalDirectIndex
            };

            // С GIN-индексом.
            _reducedSearchGin = new ReducedSearchGin
            {
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = _ginReduced
            };

            // С GIN-индексом.
            _reducedSearchGinOptimized = new ReducedSearchGinOptimized
            {
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = _ginReduced
            };

            // С GIN-индексом.
            _reducedSearchGinFast = new ReducedSearchGinFast
            {
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = _ginReduced
            };
        }

        public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
        {
            return searchType switch
            {
                ReducedSearchType.Original => _reducedSearch,
                ReducedSearchType.GinSimple => _reducedSearchGin,
                ReducedSearchType.GinOptimized => _reducedSearchGinOptimized,
                ReducedSearchType.GinFast => _reducedSearchGinFast,
                _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                    "unknown search type")
            };
        }

        public void AddVector(DocumentId documentId, TokenVector tokenVector)
        {
            _ginReduced.AddVector(documentId, tokenVector);
        }

        public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
        {
            _ginReduced.UpdateVector(documentId, tokenVector, oldTokenVector);
        }

        public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
        {
            _ginReduced.RemoveVector(documentId, tokenVector);
        }

        public void Clear()
        {
            _ginReduced.Clear();
        }
    }
}
