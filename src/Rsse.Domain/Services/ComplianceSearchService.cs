using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Services;

/// <summary>
/// Функционал поиска заметок.
/// </summary>
public sealed class ComplianceSearchService(ITokenizerService tokenizer)
{
    /// <summary>
    /// Порог актуального значения индекса. Низкий вес не стоит учитывать, если результатов много.
    /// </summary>
    private const double Threshold = 0.1D;

    /// <summary>
    /// Вычислить индексы соответствия заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска совпадений.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Идентификаторы заметок с индексами соответствия.</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new Dictionary<int, double>();
        }

        // ReSharper disable once SuggestVarOrType_Elsewhere
        Dictionary<int, double> searchIndexes = tokenizer.ComputeComplianceIndices(text, ct);
        switch (searchIndexes.Count)
        {
            case 0:
                return searchIndexes;

            case > 10:
                searchIndexes = searchIndexes
                    .Where(kv => kv.Value > Threshold)
                    .ToDictionary(x => x.Key, x => x.Value);
                break;
        }

        // todo: поведение не гарантировано, лучше использовать список
        searchIndexes = searchIndexes
            .OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        return searchIndexes;
    }
}
