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
    /// Порог актуального значения индекса. Низкий вес не стоит учитывать, если результатов много.
    /// </summary>
    private const double Threshold = 0.1D;

    /// <summary>
    /// Вычислить индексы соответствия заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска совпадений.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификаторы заметок с индексами соответствия.</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        // todo: поведение не гарантировано, лучше использовать список

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
            case > 10:
            {
                return searchIndexes
                    .Where(kv => kv.Value > Threshold)
                    .OrderByDescending(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            default:
            {
                return searchIndexes
                    .OrderByDescending(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}
