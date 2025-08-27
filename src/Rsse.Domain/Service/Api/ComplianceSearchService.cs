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
    public List<KeyValuePair<int, double>> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        // todo: поведение не гарантировано, лучше использовать список

        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var searchIndexes = tokenizer.ComputeComplianceIndices(text, cancellationToken);

        switch (searchIndexes.Count)
        {
            case 0:
                {
                    return [];
                }
            case > PageSizeThreshold:
                {
                    return searchIndexes
                        .Where(kv => kv.Value > RelevanceThreshold)
                        .OrderByDescending(x => x.Value)
                        .ToList();
                }
            default:
                {
                    return searchIndexes
                        .OrderByDescending(x => x.Value)
                        .ToList();
                }
        }
    }
}
