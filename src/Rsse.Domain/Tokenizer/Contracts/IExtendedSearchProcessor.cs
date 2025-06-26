using System.Threading;

namespace SearchEngine.Tokenizer.Contracts;

/// <summary>
/// Контракт алгоритма поиска и подсчёта расширенной метрики.
/// </summary>
public interface IExtendedSearchProcessor
{
    /// <summary>
    /// Выполнить extended поиск, посчитать extended метрики релевантности для поискового запроса, добавить в контейнер с результатом.
    /// </summary>
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Следует ли продолжать поиск.</returns>
    bool FindExtended(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken);
}
