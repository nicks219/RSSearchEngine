using System;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Indexes;

namespace SearchEngine.Tokenizer.SearchProcessor;

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
    /// <param name="invertedIndexExtended">Поддержка GIN-индекса для расширенного поиска и метрик.</param>
    /// <param name="invertedIndexReduced">Поддержка GIN-индекса для сокращенного поиска и метрик.</param>
    /// <param name="directIndexHandler">Поддержка иИндекса для всех токенизированных заметок.</param>
    /// <param name="tokenizerProcessorFactory">Фабрика процессоров токенайзера.</param>
    /// <param name="searchType">Тип оптимизации алгоритма поиска.</param>
    /// <exception cref="ArgumentNullException">Отсутствует контейнер с GIN при его требовании в оптимизации.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип оптимизации.</exception>
    public SearchProcessorFactory(
        InvertedIndexHandler? invertedIndexExtended,
        InvertedIndexHandler? invertedIndexReduced,
        DirectIndexHandler directIndexHandler,
        ITokenizerProcessorFactory tokenizerProcessorFactory,
        SearchType searchType = SearchType.Original)
    {
        var directIndex = directIndexHandler.GetGeneralDirectIndex;
        switch (searchType)
        {
            // Без GIN-индекса.
            case SearchType.Original:
                ExtendedSearchProcessor = new ExtendedSearch
                {
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearch
                {
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            // С GIN-индексом.
            case SearchType.GinOptimized:
                FailIfProductionEnvironment(searchType);
                if (invertedIndexExtended == null || invertedIndexReduced == null)
                    throw new ArgumentNullException(nameof(searchType), $"[{nameof(SearchProcessorFactory)}] GIN is null.");

                ExtendedSearchProcessor = new ExtendedSearchGinOptimized
                {
                    InvertedIndexExtended = invertedIndexExtended,
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearchGinOptimized
                {
                    InvertedIndexReduced = invertedIndexReduced,
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            // С GIN-индексом.
            case SearchType.GinSimple:
                FailIfProductionEnvironment(searchType);
                if (invertedIndexExtended == null || invertedIndexReduced == null)
                    throw new ArgumentNullException(nameof(searchType), $"[{nameof(SearchProcessorFactory)}] GIN is null.");

                ExtendedSearchProcessor = new ExtendedSearchGin
                {
                    InvertedIndexExtended = invertedIndexExtended,
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearchGin
                {
                    InvertedIndexReduced = invertedIndexReduced,
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };
                return;

            // С GIN-индексом.
            case SearchType.GinFast:
                FailIfProductionEnvironment(searchType);
                if (invertedIndexExtended == null || invertedIndexReduced == null)
                    throw new ArgumentNullException(nameof(searchType), $"[{nameof(SearchProcessorFactory)}] GIN is null.");

                ExtendedSearchProcessor = new ExtendedSearchGinFast
                {
                    InvertedIndexExtended = invertedIndexExtended,
                    DirectIndexHandler = directIndexHandler,
                    TokenizerProcessorFactory = tokenizerProcessorFactory
                };

                ReducedSearchProcessor = new ReducedSearchGinFast
                {
                    InvertedIndexReduced = invertedIndexReduced,
                    DirectIndexHandler = directIndexHandler,
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
