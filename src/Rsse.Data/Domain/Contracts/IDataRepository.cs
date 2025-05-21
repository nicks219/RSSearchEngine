using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Context;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт репозитория для данных
/// </summary>
public interface IDataRepository : IDisposable, IAsyncDisposable
{
    // todo: MySQL WORK. DELETE
    Task CopyDbFromMysqlToNpgsql();
    BaseCatalogContext? GetReaderContext();
    BaseCatalogContext? GetPrimaryWriterContext();

    // crud:

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="noteRequest">шаблон заметки</param>
    /// <returns>идентификатор созданной заметки либо ноль в случае неудачи</returns>
    Task<int> CreateNote(NoteRequestDto noteRequest);

    /// <summary>
    /// Прочитать заметку
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <returns>кортеж с текстом и названием заметки</returns>
    Task<TextResultDto?> ReadNote(int noteId);

    /// <summary>
    /// Изменить заметку
    /// </summary>
    /// <param name="initialTags">отмеченные теги</param>
    /// <param name="noteRequest">шаблон заметки</param>
    Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest);

    /// <summary>
    /// Обновить логин и пароль
    /// </summary>
    /// <param name="credosRequest">данные авторизации</param>
    Task UpdateCredos(UpdateCredosRequestDto credosRequest);

    /// <summary>
    /// Удалить заметку
    /// </summary>
    /// <param name="noteId">шаблон заметки</param>
    /// <returns>количество удаленных записей</returns>
    Task<int> DeleteNote(int noteId);

    // common:

    /// <summary>
    /// Получить список тегов в формате "имя : количество записей"
    /// </summary>
    /// <returns>список тегов</returns>
    Task<List<string>> ReadStructuredTagList();

    /// <summary>
    /// Получить количество заметок
    /// </summary>
    /// <returns>количество заметок</returns>
    Task<int> ReadNotesCount();

    /// <summary>
    /// Прочитать все заметки
    /// </summary>
    /// <returns>список заметок</returns>
    IAsyncEnumerable<NoteEntity> ReadAllNotes();

    /// <summary>
    /// Получить идентификаторы отмеченных тегами заметок
    /// </summary>
    /// <param name="checkedTags">идентификаторы отмеченных тегов</param>
    /// <returns>идентификаторы заметок</returns>
    Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags);

    /// <summary>
    /// Получить название заметки
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <returns>название заметки</returns>
    Task<string?> ReadNoteTitle(int noteId);

    /// <summary>
    /// Получить идентификаторы тегов заметки
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <returns>идентификаторы тегов</returns>
    Task<List<int>> ReadNoteTagIds(int noteId);

    // additional:

    /// <summary>
    /// Получить страницу каталога
    /// </summary>
    /// <param name="pageNumber">номер страницы</param>
    /// <param name="pageSize">размер страницы</param>
    /// <returns></returns>
    Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize);

    /// <summary>
    /// Получить сущность с авторизованным пользователем
    /// </summary>
    /// <param name="credentialsRequest">шаблон авторизации</param>
    /// <returns>сущность авторизованного пользователя</returns>
    Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest);

    /// <summary>
    /// Создать тег если отсутствует
    /// </summary>
    /// <param name="tag">именование тега</param>
    Task CreateTagIfNotExists(string tag);
}
