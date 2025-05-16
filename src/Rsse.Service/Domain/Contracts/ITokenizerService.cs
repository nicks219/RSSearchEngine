using System.Collections.Generic;
using System.Threading.Tasks;
using SearchEngine.Domain.Entities;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт сервиса поддержки токенизации заметок
/// </summary>
public interface ITokenizerService
{
    /// <summary>
    /// Создать вектор для заметки
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <param name="note">заметка</param>
    public Task Create(int id, NoteEntity note);

    /// <summary>
    /// Вычислить индексы соответствия хранимых заметок поисковому запросу
    /// </summary>
    /// <param name="text">текст для поиска соответствий</param>
    /// <returns>идентификаторы заметок и их индексы соответствия</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text);

    /// <summary>
    /// Обновить вектор для заметки
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <param name="note">заметка</param>
    public Task Update(int id, NoteEntity note);

    /// <summary>
    /// Удалить вектор для заметки
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    public Task Delete(int id);

    /// <summary>
    /// Инициализация функционала токенизации
    /// </summary>
    public Task Initialize();

    /// <summary>
    /// Дождаться инициализации токенизатора
    /// </summary>
    public Task WaitWarmUp();
}
