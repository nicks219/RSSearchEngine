using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Rsse.Domain.Service.Configuration;
using RsseEngine.Contracts;
using RsseEngine.Dto.Common;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.SearchType;
using RsseEngine.Selector;
using RsseEngine.Tokenizer.Common;
using RsseEngine.Tokenizer.Contracts;
using RsseEngine.Tokenizer.Processor;

namespace RsseEngine.Service;

/// <summary>
/// Доступ к функционалу различных алгоритмов поиска.
/// </summary>
public sealed class SearchEngineManager
{
    private const int PoolSizeThreshold = 1_000_000;

    private readonly SimpleStoragePools _simpleStoragePools;

    private readonly ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor> _extendedSearchAlgorithmSelector;

    private readonly ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor> _reducedSearchAlgorithmSelector;

    /// <summary>
    /// Токенизатор с расширенным набором символов.
    /// </summary>
    private readonly TokenizerProcessor.Extended _extendedTokenizer = new();

    /// <summary>
    /// Токенизатор с урезанным набором символов.
    /// </summary>
    private readonly TokenizerProcessor.Reduced _reducedTokenizer = new();

    /// <summary>
    /// Общий индекс для всех токенизированных заметок.
    /// </summary>
    private readonly DirectIndex _generalDirectIndex = new();

    /// <summary>
    /// Инициализация требуемого типа алгоритма.
    /// </summary>
    /// <param name="enableTempStoragePool">Пул активирован.</param>
    /// <exception cref="ArgumentNullException">Отсутствует контейнер с GIN при его требовании в оптимизации.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип оптимизации.</exception>
    public SearchEngineManager(bool enableTempStoragePool)
    {
        _simpleStoragePools = new SimpleStoragePools();

        if (EnvironmentReporter.IsProduction())
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

            _reducedSearchAlgorithmSelector = new ReducedSearchAlgorithmSelector(
                tempStoragePool, _generalDirectIndex, MetricsCalculator.ReducedCoefficient);
        }
    }

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
        if (!_generalDirectIndex.TryUpdate(documentId, tokenLine))
        {
            return false;
        }

        _extendedSearchAlgorithmSelector.UpdateVector(documentId, tokenLine.Extended);
        _reducedSearchAlgorithmSelector.UpdateVector(documentId, tokenLine.Reduced);

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
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент с метриками релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public void FindExtended(ExtendedSearchType extendedSearchType, string text,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var tokens = _simpleStoragePools.ListPool.Get();

        try
        {
            _extendedTokenizer.TokenizeText(tokens, text);

            if (tokens.Count == 0)
            {
                // заметки вида "123 456" не ищем, так как получим весь каталог
                return;
            }

            var extendedSearchVector = new TokenVector(tokens);

            _extendedSearchAlgorithmSelector.Find(extendedSearchType,
                extendedSearchVector, metricsCalculator, cancellationToken);
        }
        finally
        {
            // большие коллекции в пул не возвращаем
            if (tokens.Count < PoolSizeThreshold)
            {
                _simpleStoragePools.ListPool.Return(tokens);
            }
        }
    }

    /// <summary>
    /// Найти текст в reduced-векторах и добавить результат поиска в сокращенную метрику.
    /// </summary>
    /// <param name="reducedSearchType">Тип оптимизации алгоритма поиска.</param>
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент с метриками релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public void FindReduced(ReducedSearchType reducedSearchType, string text,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var textTokens = _simpleStoragePools.ListPool.Get();
        var uniqueTokens = _simpleStoragePools.SetPool.Get();
        var vectorTokens = _simpleStoragePools.ListPool.Get();

        try
        {
            _reducedTokenizer.TokenizeText(textTokens, text);

            if (textTokens.Count == 0)
            {
                // песни вида "123 456" не ищем, так как получим весь каталог
                return;
            }

            // убираем дубликаты слов, это меняет результаты поиска (тексты типа "казино казино казино")
            foreach (var token in textTokens)
            {
                if (uniqueTokens.Add(token))
                {
                    vectorTokens.Add(token);
                }
            }

            var reducedSearchVector = new TokenVector(vectorTokens);

            _reducedSearchAlgorithmSelector.Find(reducedSearchType,
                reducedSearchVector, metricsCalculator, cancellationToken);
        }
        finally
        {
            // большие коллекции в пул не возвращаем
            if (textTokens.Count < PoolSizeThreshold)
            {
                _simpleStoragePools.ListPool.Return(textTokens);
                _simpleStoragePools.SetPool.Return(uniqueTokens);
                _simpleStoragePools.ListPool.Return(vectorTokens);
            }
        }
    }

    /// <summary>
    /// Создать extended-вектор токенов для заметки.
    /// </summary>
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <returns>Вектор токенов, представляющий обработанный текст.</returns>
    public TokenVector TokenizeTextExtended(params string[] text)
    {
        return TokenizeText(_extendedTokenizer, text);
    }

    /// <summary>
    /// Создать reduced-вектор токенов для заметки.
    /// </summary>
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <returns>Вектор токенов, представляющий обработанный текст.</returns>
    public TokenVector TokenizeTextReduced(params string[] text)
    {
        return TokenizeText(_reducedTokenizer, text);
    }

    private TokenVector TokenizeText(ITokenizerProcessor tokenizerProcessor, params string[] text)
    {
        var tokensList = _simpleStoragePools.ListPool.Get();

        try
        {
            tokenizerProcessor.TokenizeText(tokensList, text);

            var tokens = new List<int>(tokensList.Count);
            CollectionsMarshal.SetCount(tokens, tokensList.Count);

            var destination = CollectionsMarshal.AsSpan(tokens);
            tokensList.CopyTo(destination);

            var tokenVector = new TokenVector(tokens);

            return tokenVector;
        }
        finally
        {
            _simpleStoragePools.ListPool.Return(tokensList);
        }
    }
}
