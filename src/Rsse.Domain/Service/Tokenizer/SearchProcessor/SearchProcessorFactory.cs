using System;
using System.Collections.Concurrent;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Функционал, поставляющий различные алгоритмы вычисления метрик поиска.
/// Для бенчмарков.
/// </summary>
public sealed class SearchProcessorFactory
{
    /// <summary>
    /// Алгоритм поиска текста в extended-векторах и подсчёта расширенной метрики.
    /// </summary>
    public IExtendedSearchProcessor ExtendedSearchProcessor { get; }

    /// <summary>
    /// Алгоритм поиска текста в reduced-векторах и подсчёта сокращенной метрики.
    /// </summary>
    public IReducedSearchProcessor ReducedSearchProcessor { get; }

    /// <summary>
    /// Инициализация требуемого типа алгоритма.
    /// </summary>
    /// <param name="ginExtended">Поддержка GIN-индекса для расширенного поиска и метрик.</param>
    /// <param name="ginReduced">Поддержка GIN-индекса для сокращенного поиска и метрик.</param>
    /// <param name="generalDirectIndex">Индекс для всех токенизированных заметок.</param>
    /// <param name="tokenizerProcessorFactory">Фабрика процессоров токенайзера.</param>
    /// <param name="searchType">Тип оптимизации алгоритма поиска.</param>
    /// <exception cref="ArgumentNullException">Отсутствует контейнер с GIN при его требовании в оптимизации.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип оптимизации.</exception>
    public SearchProcessorFactory(
        GinHandler<DocumentIdSet>? ginExtended,
        GinHandler<DocumentIdSet>? ginReduced,
        ConcurrentDictionary<DocumentId, TokenLine> generalDirectIndex,
        ITokenizerProcessorFactory tokenizerProcessorFactory,
        SearchType searchType = SearchType.Original)
    {
        switch (searchType)
        {
            // Без GIN-индекса.
            case SearchType.Original:
                ExtendedSearchProcessor = new ExtendedSearch
                {
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearch
                {
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            // С GIN-индексом.
            case SearchType.GinOptimized:
                FailIfProductionEnvironment(searchType);
                if (ginExtended == null || ginReduced == null)
                    throw new ArgumentNullException(nameof(searchType), $"[{nameof(SearchProcessorFactory)}] GIN is null.");

                ExtendedSearchProcessor = new ExtendedSearchGinOptimized
                {
                    GinExtended = ginExtended,
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearchGinOptimized
                {
                    GinReduced = ginReduced,
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            // С GIN-индексом.
            case SearchType.GinSimple:
                FailIfProductionEnvironment(searchType);
                if (ginExtended == null || ginReduced == null)
                    throw new ArgumentNullException(nameof(searchType), $"[{nameof(SearchProcessorFactory)}] GIN is null.");

                ExtendedSearchProcessor = new ExtendedSearchGin
                {
                    GinExtended = ginExtended,
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearchGin
                {
                    GinReduced = ginReduced,
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            // С GIN-индексом.
            case SearchType.GinFast:
                FailIfProductionEnvironment(searchType);
                if (ginExtended == null || ginReduced == null)
                    throw new ArgumentNullException(nameof(searchType), $"[{nameof(SearchProcessorFactory)}] GIN is null.");

                ExtendedSearchProcessor = new ExtendedSearchGinFast
                {
                    GinExtended = ginExtended,
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearchGinFast
                {
                    GinReduced = ginReduced,
                    GeneralDirectIndex = generalDirectIndex,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "unknown search type");
        }
    }

    /// <summary>
    /// Упасть при запуске в производственном окружении.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    private static void FailIfProductionEnvironment(SearchType searchType)
    {
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "production";
        if (!isProduction)
        {
            return;
        }

        throw new NotSupportedException($"[{searchType.ToString()}] GIN optimization is not supported in production yet.");
    }
}
