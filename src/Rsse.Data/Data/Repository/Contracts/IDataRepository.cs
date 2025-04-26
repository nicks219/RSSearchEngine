using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Context;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;

namespace SearchEngine.Data.Repository.Contracts;

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
    /// <param name="note">шаблон заметки</param>
    /// <returns>идентификатор созданной заметки либо ноль в случае неудачи</returns>
    Task<int> CreateNote(NoteDto note);

    /// <summary>
    /// Прочитать заметку
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <returns>кортеж с текстом и названием заметки</returns>
    IQueryable<Tuple<string, string>> ReadNote(int noteId);

    /// <summary>
    /// Изменить заметку
    /// </summary>
    /// <param name="initialTags">отмеченные теги</param>
    /// <param name="note">шаблон заметки</param>
    Task UpdateNote(IEnumerable<int> initialTags, NoteDto note);

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
    IQueryable<NoteEntity> ReadAllNotes();

    /// <summary>
    /// Получить отмеченные заметки
    /// </summary>
    /// <param name="checkedTags">идентификаторы отмеченных тегов</param>
    /// <returns>идентификаторы заметок</returns>
    IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags);

    /// <summary>
    /// Получить название заметки
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <returns>название заметки</returns>
    string ReadNoteTitle(int noteId);

    /// <summary>
    /// Получить идентификатор заметки
    /// </summary>
    /// <param name="noteTitle">название заметки</param>
    /// <returns>идентификатор заметки</returns>
    int ReadNoteId(string noteTitle);

    /// <summary>
    /// Получить теги заметки
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <returns>идентификаторы тегов</returns>
    IQueryable<int> ReadNoteTags(int noteId);

    // additional:

    /// <summary>
    /// Получить страницу каталога
    /// </summary>
    /// <param name="pageNumber">номер страницы</param>
    /// <param name="pageSize">размер страницы</param>
    /// <returns></returns>
    IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize);

    /// <summary>
    /// Получить сущность с авторизованным пользователем
    /// </summary>
    /// <param name="login">шаблон авторизации</param>
    /// <returns>сущность авторизованного пользователя</returns>
    Task<UserEntity?> GetUser(LoginDto login);

    /// <summary>
    /// Создать тег если отсутствует
    /// </summary>
    /// <param name="tag">именование тега</param>
    Task CreateTagIfNotExists(string tag);
}
