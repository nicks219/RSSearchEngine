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
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Идентификатор созданной заметки, либо ноль в случае неудачи.</returns>
    Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken ct);

    /// <summary>
    /// Прочитать заметку по идентификатору.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Контейнер ответа с текстовой нагрузкой заметки.</returns>
    Task<TextResultDto?> ReadNote(int noteId, CancellationToken ct);

    /// <summary>
    /// Изменить заметку.
    /// </summary>
    /// <param name="initialTags">Отмеченные теги.</param>
    /// <param name="noteRequest">Контейнер для запроса с заметкой.</param>
    /// <param name="ct">Токен отмены.</param>
    Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest, CancellationToken ct);

    /// <summary>
    /// Обновить логин и пароль.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <param name="credosRequest">Контейнер с данными авторизации.</param>
    Task UpdateCredos(UpdateCredosRequestDto credosRequest, CancellationToken ct);

    /// <summary>
    /// Удалить заметку по идентификатору.
    /// </summary>
    /// <param name="noteId">Идетнификатор заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Количество удаленных записей.</returns>
    Task<int> DeleteNote(int noteId, CancellationToken ct);

    // common:

    /// <summary>
    /// Получить список тегов в формате "имя : количество записей".
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Обогащенный список тегов.</returns>
    Task<List<string>> ReadEnrichedTagList(CancellationToken ct);

    /// <summary>
    /// Получить общее количество заметок.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Количество заметок.</returns>
    Task<int> ReadNotesCount(CancellationToken ct);

    /// <summary>
    /// Прочитать все заметки.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Список заметок.</returns>
    ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken ct);

    /// <summary>
    /// Получить идентификаторы заметок, отмеченных тегами.
    /// </summary>
    /// <param name="checkedTags">Идентификаторы тегов для выбора заметок.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Идентификаторы отмеченных заметок.</returns>
    Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken ct);

    /// <summary>
    /// Получить название заметки по идентификатору.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Название заметки.</returns>
    Task<string?> ReadNoteTitle(int noteId, CancellationToken ct);

    /// <summary>
    /// Получить идентификаторы тегов заметки по идентификатору заметки.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Идентификаторы тегов.</returns>
    Task<List<int>> ReadNoteTagIds(int noteId, CancellationToken ct);

    // additional:

    /// <summary>
    /// Получить страницу каталога.
    /// </summary>
    /// <param name="pageNumber">Номер страницы каталога.</param>
    /// <param name="pageSize">Требуемое количество заметок на странице каталога.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Список заметок, представляющий страницу каталога.</returns>
    Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize, CancellationToken ct);

    /// <summary>
    /// Получить сущность с авторизованным пользователем.
    /// </summary>
    /// <param name="credentialsRequest">Контейнер с данными авторизации.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Сущность бд с авторизованным пользователем.</returns>
    Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest, CancellationToken ct);

    /// <summary>
    /// Создать тег если отсутствует.
    /// </summary>
    /// <param name="tag">Именование тега.</param>
    /// <param name="ct">Токен отмены.</param>
    Task CreateTagIfNotExists(string tag, CancellationToken ct);
}
