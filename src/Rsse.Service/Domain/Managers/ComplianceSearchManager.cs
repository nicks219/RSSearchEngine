using System.Collections.Generic;
using SearchEngine.Domain.Contracts;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал поиска заметок
/// </summary>
public class ComplianceSearchManager(ITokenizerService tokenizer)
{
    /// <summary>
    /// Вычислить индексы соответствия хранимых заметок поисковому запросу
    /// </summary>
    /// <param name="text">текст для поиска соответствий</param>
    /// <returns>идентификаторы заметок и их индексы соответствия</returns>
    // todo: это read для ITokenizerService, подумай как лучше затащить в него этот метод
    public Dictionary<int, double> ComputeComplianceIndices(string text)
    {
        var result = tokenizer.ComputeComplianceIndices(text);
        return result;
    }
}
