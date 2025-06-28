using System.Threading;
using RsseEngine.Dto;

namespace RsseEngine.Contracts;

/// <summary>
/// Контракт поиска с ответом в виде расширенной метрики.
/// </summary>
public interface IExtendedSearchProcessor
{
    /// <summary>
    /// Выполнить extended-поиск и посчитать extended метрики релевантности для поискового запроса.
    /// Добавить метрики в контейнер с результатом.
    /// </summary>
    /// <param name="searchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    void FindExtended(TokenVector searchVector,
        IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);
}
