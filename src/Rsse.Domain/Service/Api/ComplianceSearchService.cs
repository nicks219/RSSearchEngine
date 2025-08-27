using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rsse.Domain.Service.Contracts;

namespace Rsse.Domain.Service.Api;

/// <summary>
/// Функционал поиска заметок.
/// </summary>
public sealed class ComplianceSearchService(ITokenizerApiClient tokenizer)
{
    /// <summary>
    /// Количество элементов, после которого произойдёт отсеивание по пороговому значению релевантности.
    /// </summary>
    public const int PageSizeThreshold = 10;

    /// <summary>
    /// Порог актуального значения релевантности, ниже которого результаты не будут учитываться, если их много.
    /// </summary>
    private const double RelevanceThreshold = 0.1D;

    /// <summary>
    /// Вычислить индексы соответствия заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска совпадений.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификаторы заметок с индексами соответствия.</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        // TODO нужно переписать сериализацию что бы возвращать лист

        // TODO: поведение не гарантировано, лучше использовать список

        if (string.IsNullOrEmpty(text))
        {
            return new Dictionary<int, double>();
        }

        var searchIndexes = tokenizer.ComputeComplianceIndices(text, cancellationToken);

        switch (searchIndexes.Count)
        {
            case 0:
                {
                    return new Dictionary<int, double>();
                }
            case > PageSizeThreshold: // TODO этот код больше не нужен - PageSizeThreshold делается в MetricsCalculator
                {
                    return searchIndexes
                        .Where(kv => kv.Value > RelevanceThreshold)
                        .OrderByDescending(x => x.Value)
                        .ToDictionary(x => x.Key, x => x.Value);
                }
            default: // TODO сортировка уже сделана в MetricsCalculator
                {
                    return searchIndexes
                        .OrderByDescending(x => x.Value)
                        .ToDictionary(x => x.Key, x => x.Value);
                }
        }
    }
}
