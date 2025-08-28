using System.Collections.Generic;
using System.Threading;
using Rsse.Domain.Service.Contracts;

namespace Rsse.Domain.Service.Api;

/// <summary>
/// Функционал поиска заметок.
/// </summary>
public sealed class ComplianceSearchService(ITokenizerApiClient tokenizerClient)
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
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var searchIndexes = tokenizerClient.ComputeComplianceIndices(text, cancellationToken);

        switch (searchIndexes.Count)
        {
            case > PageSizeThreshold:
                for (var index = searchIndexes.Count - 1; index >= 0; index--)
                {
                    if (searchIndexes[index].Value <= RelevanceThreshold)
                    {
                        searchIndexes.RemoveAt(index);
                    }
                }

                return searchIndexes;
            default:
                return searchIndexes;
        }
    }
}
