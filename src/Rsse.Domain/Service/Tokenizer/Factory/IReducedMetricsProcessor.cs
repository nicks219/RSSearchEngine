using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Контракт алгоритма поиска и подсчёта сокращенной метрики.
/// </summary>
public interface IReducedMetricsProcessor
{
    /// <summary>
    /// Выполнить reduced поиск, посчитать reduced метрики релевантности для поискового запроса, добавить в контейнер с результатом.
    /// </summary>
    /// <param name="text">Текст с поисковым запросом.</param>
    /// <param name="complianceMetrics">Контейнер с ответом в виде индекса релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    void FindReduced(string text, Dictionary<DocId, double> complianceMetrics, CancellationToken cancellationToken);
}
