using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.SearchType;
using RsseEngine.Selector;
using RsseEngine.Tokenizer.Contracts;
using RsseEngine.Tokenizer.Processor;

namespace RsseEngine.Service;

/// <summary>
/// Доступ к функционалу различных алгоритмов поиска.
/// </summary>
public sealed class SearchEngineManager
{
    private readonly ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor> _extendedSearchAlgorithmSelector;

    private readonly ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor> _reducedSearchAlgorithmSelector;

    /// <summary>
    /// Общий индекс для всех токенизированных заметок.
    /// </summary>
    private readonly DirectIndex _generalDirectIndex = new();

    /// <summary>
    /// Инициализация требуемого типа алгоритма.
    /// </summary>
    /// <param name="enableTempStoragePool">Пул активирован.</param>
    /// <param name="useList">Используем DocumentIdList иначе DocumentIdSet</param>
    /// <exception cref="ArgumentNullException">Отсутствует контейнер с GIN при его требовании в оптимизации.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип оптимизации.</exception>
    public SearchEngineManager(bool enableTempStoragePool, bool useList)
    {
        if (CheckIsProduction())
        {
            _extendedSearchAlgorithmSelector =
                new ProductionSearchAlgorithmSelector.ExtendedLegacy(_generalDirectIndex);

            _reducedSearchAlgorithmSelector =
                new ProductionSearchAlgorithmSelector.ReducedLegacy(_generalDirectIndex);
        }
        else
        {
            var tempStoragePool = new TempStoragePool(enableTempStoragePool);

            _extendedSearchAlgorithmSelector = new ExtendedSearchAlgorithmSelector(
                tempStoragePool, _generalDirectIndex, MetricsCalculator.ExtendedCoefficient);

            if (useList)
            {
                _reducedSearchAlgorithmSelector = new ReducedSearchAlgorithmSelector<DocumentIdList>(
                    tempStoragePool, _generalDirectIndex, MetricsCalculator.ReducedCoefficient);
            }
            else
            {
                _reducedSearchAlgorithmSelector = new ReducedSearchAlgorithmSelector<DocumentIdSet>(
                    tempStoragePool, _generalDirectIndex, MetricsCalculator.ReducedCoefficient);
            }
        }
    }

    /// <summary>
    /// Токенизатор с расширенным набором символов.
    /// </summary>
    public ITokenizerProcessor ExtendedTokenizer { get; } = new TokenizerProcessor.Extended();

    /// <summary>
    /// Токенизатор с урезанным набором символов.
    /// </summary>
    public ITokenizerProcessor ReducedTokenizer { get; } = new TokenizerProcessor.Reduced();

    /// <summary>
    /// Получить размер общего индекса.
    /// </summary>
    public int DirectIndexCount => _generalDirectIndex.Count;

    /// <summary>
    /// Получить общий индекс.
    /// </summary>
    public DirectIndex GetDirectIndex() => _generalDirectIndex;

    /// <summary>
    /// Добавить в индексы идентификатор документа и его extended/reduced векторы.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenLine">Контейнер с extended/reduced векторами.</param>
    /// <returns>Признак успешного выполнения.</returns>
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

    /// <summary>
    /// Обновить в индексах extended/reduced векторы для заданного документа.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenLine">Контейнер с extended/reduced векторами.</param>
    /// <returns>Признак успешного выполнения.</returns>
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

    /// <summary>
    /// Удалить из индексов требуемый документ.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <returns>Признак успешного выполнения.</returns>
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

    /// <summary>
    /// Очистить общие индексы.
    /// </summary>
    public void Clear()
    {
        _generalDirectIndex.Clear();
        _extendedSearchAlgorithmSelector.Clear();
        _reducedSearchAlgorithmSelector.Clear();
    }

    /// <summary>
    /// Найти текст в extended-векторах и добавить результат поиска в расширенную метрику.
    /// </summary>
    /// <param name="extendedSearchType">Тип оптимизации алгоритма поиска.</param>
    /// <param name="extendedSearchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент с метриками релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public void FindExtended(ExtendedSearchType extendedSearchType, TokenVector extendedSearchVector,
        MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        _extendedSearchAlgorithmSelector
            .GetSearchProcessor(extendedSearchType)
            .FindExtended(extendedSearchVector, metricsCalculator, cancellationToken);
    }

    /// <summary>
    /// Найти текст в reduced-векторах и добавить результат поиска в сокращенную метрику.
    /// </summary>
    /// <param name="reducedSearchType">Тип оптимизации алгоритма поиска.</param>
    /// <param name="reducedSearchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент с метриками релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public void FindReduced(ReducedSearchType reducedSearchType, TokenVector reducedSearchVector,
        MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        _reducedSearchAlgorithmSelector
            .GetSearchProcessor(reducedSearchType)
            .FindReduced(reducedSearchVector, metricsCalculator, cancellationToken);
    }

    /// <summary>
    /// Вернуть признак запуска в производственном окружении.
    /// </summary>
    private static bool CheckIsProduction()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "production";
    }
}
