using System.Collections.Concurrent;
using System.Collections.Generic;
using SearchEngine.Domain.Entities;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт сервиса поддержки токенизации заметок
/// </summary>
public interface ITokenizerService
{
    /// <summary>
    /// Получить редуцированный вектор для заметки
    /// </summary>
    /// <returns>идентификаторы заметок и соответствующие им векторы</returns>
    public ConcurrentDictionary<int, List<int>> GetReducedLines();

    /// <summary>
    /// Получить расширенный вектор для заметки
    /// </summary>
    /// <returns>идентификаторы заметок и соответствующие им векторы</returns>
    public ConcurrentDictionary<int, List<int>> GetExtendedLines();

    /// <summary>
    /// Удалить вектор для заметки
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    public void Delete(int id);

    /// <summary>
    /// Создать вектор для заметки
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <param name="note">заметка</param>
    public void Create(int id, NoteEntity note);

    /// <summary>
    /// Обновить вектор для заметки
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <param name="note">заметка</param>
    public void Update(int id, NoteEntity note);

    /// <summary>
    /// Инициализация функционала токенизации
    /// </summary>
    public void Initialize();
}
