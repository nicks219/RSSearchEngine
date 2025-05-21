using System.Collections.Generic;
using System.Threading.Tasks;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Entities;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт сервиса поддержки токенизации заметок.
/// </summary>
public interface ITokenizerService
{
    /// <summary>
    /// Создать вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    public Task Create(int id, TextRequestDto note);

    /// <summary>
    /// Вычислить индексы соответствия хранимых заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска соответствий.</param>
    /// <returns>Идентификаторы заметок и их индексы соответствия.</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text);

    /// <summary>
    /// Обновить вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    public Task Update(int id, TextRequestDto note);

    /// <summary>
    /// Удалить вектор для заметки по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    public Task Delete(int id);

    /// <summary>
    /// Инициализация функционала токенизации.
    /// </summary>
    public Task Initialize();

    /// <summary>
    /// Дождаться инициализации токенизатора.
    /// </summary>
    public Task WaitWarmUp();
}
