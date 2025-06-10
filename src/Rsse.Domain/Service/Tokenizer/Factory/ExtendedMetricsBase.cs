using System.Collections.Concurrent;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта extended метрик.
/// </summary>
public abstract class ExtendedMetricsBase
{
    // Коэффициент extended поиска: 0.8D
    protected const double ExtendedCoefficient = 0.8D;

    public required GinHandler ExtendedGin { get; init; }

    public required ITokenizerProcessorFactory ProcessorFactory { get; init; }

    public required ConcurrentDictionary<DocId, TokenLine> TokenLines { get; init; }
}
