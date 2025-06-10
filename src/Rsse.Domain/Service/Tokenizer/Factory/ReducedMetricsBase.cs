using System.Collections.Concurrent;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта reduced метрик.
/// </summary>
public class ReducedMetricsBase
{
    // Коэффициент reduced поиска: 0.4D
    protected const double ReducedCoefficient = 0.6D; // 0.6 .. 0.75


    public required GinHandler ReducedGin { get; init; }

    public required ITokenizerProcessorFactory ProcessorFactory { get; init; }

    public required ConcurrentDictionary<DocId, TokenLine> TokenLines { get; init; }
}
