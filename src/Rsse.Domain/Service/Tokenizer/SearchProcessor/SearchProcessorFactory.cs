using System;
using System.Threading;
using Rsse.Search;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Selector;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Processor;

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
            _extendedSearchAlgorithmSelector = new ProductionSearchAlgorithmSelector.Extended(_generalDirectIndex);
            _reducedSearchAlgorithmSelector = new ProductionSearchAlgorithmSelector.Reduced(_generalDirectIndex);
        }
        else
        {
            _extendedSearchAlgorithmSelector = new ExtendedSearchAlgorithmSelector(_generalDirectIndex);
            _reducedSearchAlgorithmSelector = new ReducedSearchAlgorithmSelector(_generalDirectIndex);
        }
    }

    /// <summary>
    /// Токенизатор с расширенным набором символов.
    /// </summary>
    public ITokenizerProcessor ExtendedTokenizer
    {
        get;
    } = new TokenizerProcessor.Extended();

    /// <summary>
    /// Токенизатор с урезанным набором символов.
    /// </summary>
    public ITokenizerProcessor ReducedTokenizer
    {
        get;
    } = new TokenizerProcessor.Reduced();

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
}
