using System.Threading;
using SearchEngine.Dto;

namespace SearchEngine.Contracts;

/// <summary>
/// Контракт алгоритма поиска и подсчёта расширенной метрики.
/// </summary>
public interface IExtendedSearchProcessor
{
    /// <summary>
    /// Выполнить extended поиск, посчитать extended метрики релевантности для поискового запроса, добавить в контейнер с результатом.
    /// </summary>
    /// <param name="searchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);
}
