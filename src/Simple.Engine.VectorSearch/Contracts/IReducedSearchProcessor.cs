using System.Threading;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Contracts;

/// <summary>
/// Контракт поиска с ответом в виде сокращенной метрики.
/// </summary>
public interface IReducedSearchProcessor
{
    /// <summary>
    /// Выполнить reduced-поиск и посчитать reduced метрики релевантности для поискового запроса.
    /// Добавить в контейнер с результатом.
    /// </summary>
    /// <param name="searchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    void FindReduced(TokenVector searchVector,
        IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);
}
