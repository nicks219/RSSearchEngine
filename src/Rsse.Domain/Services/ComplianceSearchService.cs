using System.Collections.Generic;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Services;

/// <summary>
/// Функционал поиска заметок.
/// </summary>
public class ComplianceSearchService(ITokenizerService tokenizer)
{
    /// <summary>
    /// Вычислить индексы соответствия заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска совпадений.</param>
    /// <returns>Идентификаторы заметок с индексами соответствия.</returns>
    // todo: это read для ITokenizerService, подумай как лучше затащить в него этот метод
    public Dictionary<int, double> ComputeComplianceIndices(string text)
    {
        var result = tokenizer.ComputeComplianceIndices(text);
        return result;
    }
}
