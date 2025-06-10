using System;
using System.Collections.Concurrent;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Функционал, поставляющий различные алгоритмы вычисления метрик поиска.
/// Для бенчмарков.
/// </summary>
public class MetricsProcessorFactory
{
    /// <summary>
    /// Алгоритм поиска текста в extended-векторах и подсчёта расширенной метрики.
    /// </summary>
    public IExtendedMetricsProcessor ExtendedMetricsProcessor { get; }

    /// <summary>
    /// Алгоритм поиска текста в reduced-векторах и подсчёта сокращенной метрики.
    /// </summary>
    public IReducedMetricsProcessor ReducedMetricsProcessor { get; }

    /// <summary>
    /// Инициализация требуемого типа алгоритма.
    /// </summary>
    /// <param name="extendedGin">GIN для расширенных метрик.</param>
    /// <param name="reducedGin">GIN для сокращенных метрик.</param>
    /// <param name="tokenLines">Токенизированный индекс всех заметок.</param>
    /// <param name="processorFactory">Фабрика процессоров токенайзера.</param>
    /// <param name="searchType">Тип оптимизации алгоритма поиска.</param>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип оптимизации.</exception>
    // todo: попробовать сделать на билдере.
    public MetricsProcessorFactory(
        GinHandler extendedGin,
        GinHandler reducedGin,
        ConcurrentDictionary<DocId, TokenLine> tokenLines,
        ITokenizerProcessorFactory processorFactory,
        SearchType searchType = SearchType.Original)
    {
        switch (searchType)
        {
            // Без GIN-индекса.
            case SearchType.Original:
                ExtendedMetricsProcessor = new ExtendedMetricsOriginal
                {
                    ExtendedGin = extendedGin,
                    TokenLines = tokenLines,
                    ProcessorFactory = processorFactory
                };

                ReducedMetricsProcessor = new ReducedMetricsOriginal
                {
                    ReducedGin = reducedGin,
                    TokenLines = tokenLines,
                    ProcessorFactory = processorFactory
                };
                break;

            // С GIN-индексом.
            case SearchType.GinOptimized:
                FailIfProductionEnvironment(searchType);

                ExtendedMetricsProcessor = new ExtendedSearchGinOptimized
                {
                    ExtendedGin = extendedGin,
                    TokenLines = tokenLines,
                    ProcessorFactory = processorFactory
                };

                ReducedMetricsProcessor = new ReducedMetricsGinOptimized
                {
                    ReducedGin = reducedGin,
                    TokenLines = tokenLines,
                    ProcessorFactory = processorFactory
                };
                break;

            // С GIN-индексом.
            case SearchType.GinSimple:
                FailIfProductionEnvironment(searchType);

                ExtendedMetricsProcessor = new ExtendedMetricsGin
                {
                    ExtendedGin = extendedGin,
                    TokenLines = tokenLines,
                    ProcessorFactory = processorFactory
                };

                ReducedMetricsProcessor = new ReducedMetricsGin
                {
                    ReducedGin = reducedGin,
                    TokenLines = tokenLines,
                    ProcessorFactory = processorFactory
                };
                break;

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
