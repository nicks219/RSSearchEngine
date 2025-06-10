using System.Threading;

namespace SearchEngine.Service.Tokenizer.Contracts;

/// <summary>
/// Контракт алгоритма поиска и подсчёта сокращенной метрики.
/// </summary>
public interface IReducedSearchProcessor
{
    /// <summary>
    /// Выполнить reduced поиск, посчитать reduced метрики релевантности для поискового запроса, добавить в контейнер с результатом.
    /// </summary>
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    void FindReduced(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken);
}
