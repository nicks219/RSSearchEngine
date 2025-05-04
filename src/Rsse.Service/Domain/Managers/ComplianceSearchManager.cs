using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Domain.Contracts;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал поиска заметок
/// </summary>
public class ComplianceSearchManager(IServiceProvider scopedProvider)
{
    private readonly IDataRepository _repo = scopedProvider.GetRequiredService<IDataRepository>();

    /// <summary>
    /// Найти идентификатор заметки по её имени, требуется только для тестов
    /// </summary>
    /// <param name="name">имя заметки</param>
    /// <returns>идентификатор заметки</returns>
    public int FindNoteId(string name) => _repo.ReadNoteId(name);

    /// <summary>
    /// Вычислить индексы соответствия хранимых заметок поисковому запросу
    /// </summary>
    /// <param name="text">текст для поиска соответствий</param>
    /// <returns>идентификаторы заметок и их индексы соответствия</returns>
    // todo: это read для ITokenizerService, подумай как лучше затащить в него этот метод
    public Dictionary<int, double> ComputeComplianceIndices(string text)
    {
        var tokenizer = scopedProvider.GetRequiredService<ITokenizerService>();
        var result = tokenizer.ComputeComplianceIndices(text);
        return result;
    }
}
