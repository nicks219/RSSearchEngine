using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;

namespace SearchEngine.Data.Contracts;

/// <summary>
/// Контракт репозитория для данных.
/// </summary>
public interface IDataRepository
{
    /// <summary>
    /// Создать заметку.
    /// </summary>
    /// <param name="noteRequest">Контейнер запроса с заметкой.</param>
    /// <returns>Идентификатор созданной заметки, либо ноль в случае неудачи.</returns>
    Task<int> CreateNote(NoteRequestDto noteRequest);

    /// <summary>
    /// Прочитать заметку по идентификатору.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <returns>Контейнер ответа с текстовой нагрузкой заметки.</returns>
    Task<TextResultDto?> ReadNote(int noteId);

    /// <summary>
    /// Изменить заметку.
    /// </summary>
    /// <param name="initialTags">Отмеченные теги.</param>
    /// <param name="noteRequest">Контейнер для запроса с заметкой.</param>
    Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest);

    /// <summary>
    /// Обновить логин и пароль.
    /// </summary>
    /// <param name="credosRequest">Контейнер с данными авторизации.</param>
    Task UpdateCredos(UpdateCredosRequestDto credosRequest);

    /// <summary>
    /// Удалить заметку по идентификатору.
    /// </summary>
    /// <param name="noteId">Идетнификатор заметки.</param>
    /// <returns>Количество удаленных записей.</returns>
    Task<int> DeleteNote(int noteId);

    // common:

    /// <summary>
    /// Получить список тегов в формате "имя : количество записей".
    /// </summary>
    /// <returns>Обогащенный список тегов.</returns>
    Task<List<string>> ReadEnrichedTagList();

    /// <summary>
    /// Получить общее количество заметок.
    /// </summary>
    /// <returns>Количество заметок.</returns>
    Task<int> ReadNotesCount();

    /// <summary>
    /// Прочитать все заметки.
    /// </summary>
    /// <returns>Список заметок.</returns>
    ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken ct);

    /// <summary>
    /// Получить идентификаторы заметок, отмеченных тегами.
    /// </summary>
    /// <param name="checkedTags">Идентификаторы тегов для выбора заметок.</param>
    /// <returns>Идентификаторы отмеченных заметок.</returns>
    Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags);

    /// <summary>
    /// Получить название заметки по идентификатору.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <returns>Название заметки.</returns>
    Task<string?> ReadNoteTitle(int noteId);

    /// <summary>
    /// Получить идентификаторы тегов заметки по идентификатору заметки.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <returns>Идентификаторы тегов.</returns>
    Task<List<int>> ReadNoteTagIds(int noteId);

    // additional:

    /// <summary>
    /// Получить страницу каталога.
    /// </summary>
    /// <param name="pageNumber">Номер страницы каталога.</param>
    /// <param name="pageSize">Требуемое количество заметок на странице каталога.</param>
    /// <returns>Список заметок, представляющий страницу каталога.</returns>
    Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize);

    /// <summary>
    /// Получить сущность с авторизованным пользователем.
    /// </summary>
    /// <param name="credentialsRequest">Контейнер с данными авторизации.</param>
    /// <returns>Сущность бд с авторизованным пользователем.</returns>
    Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest);

    /// <summary>
    /// Создать тег если отсутствует.
    /// </summary>
    /// <param name="tag">Именование тега.</param>
    Task CreateTagIfNotExists(string tag);
}
