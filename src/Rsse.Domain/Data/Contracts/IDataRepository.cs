using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Data.Entities;

namespace Rsse.Domain.Data.Contracts;

/// <summary>
/// Контракт репозитория для данных.
/// </summary>
public interface IDataRepository
{
    /// <summary>
    /// Получить случайную заметку, используя средства SQL.
    /// </summary>
    /// <param name="checkedTags">Идентификаторы тегов для выбора заметок.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Случайная заметка.</returns>
    Task<NoteEntity?> GetRandomNoteOrDefault(IEnumerable<int> checkedTags, CancellationToken cancellationToken);

    /// <summary>
    /// Создать заметку.
    /// </summary>
    /// <param name="noteRequest">Контейнер запроса с заметкой.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Идентификатор созданной заметки, либо ноль в случае неудачи.</returns>
    Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken stoppingToken);

    /// <summary>
    /// Прочитать заметку по идентификатору.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Контейнер ответа с текстовой нагрузкой заметки.</returns>
    Task<TextResultDto?> ReadNote(int noteId, CancellationToken cancellationToken);

    /// <summary>
    /// Изменить заметку.
    /// </summary>
    /// <param name="noteRequest">Контейнер для запроса с заметкой.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    Task UpdateNote(NoteRequestDto noteRequest, CancellationToken stoppingToken);

    /// <summary>
    /// Обновить логин и пароль.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <param name="credosRequest">Контейнер с данными авторизации.</param>
    Task UpdateCredos(UpdateCredosRequestDto credosRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить заметку по идентификатору.
    /// </summary>
    /// <param name="noteId">Идетнификатор заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Количество удаленных записей.</returns>
    Task<int> DeleteNote(int noteId, CancellationToken stoppingToken);

    // common:

    /// <summary>
    /// Получить список тегов, поддерживающих конвертацию в формат "имя : количество записей".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список тегов, поддерживающих конвертацию.</returns>
    Task<List<TagResultDto>> ReadTags(CancellationToken cancellationToken);

    /// <summary>
    /// Получить список тегов с признаками соответствия/несоответствия конкретной заметке.
    /// </summary>
    /// <param name="nodeId">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список тегов с признаками соответствия/несоответствия.</returns>
    Task<List<TagMarkedResultDto>> ReadMarkedTags(int nodeId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить общее количество заметок.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество заметок.</returns>
    Task<int> ReadNotesCount(CancellationToken cancellationToken);

    /// <summary>
    /// Прочитать все заметки.
    /// </summary>
    /// <returns>Список заметок.</returns>
    IAsyncEnumerable<NoteEntity> ReadAllNotes();

    /// <summary>
    /// Получить идентификаторы заметок, отмеченных тегами.
    /// </summary>
    /// <param name="checkedTags">Идентификаторы тегов для выбора заметок.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификаторы отмеченных заметок.</returns>
    Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken cancellationToken);

    /// <summary>
    /// Получить название заметки по идентификатору.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Название заметки.</returns>
    Task<string?> ReadNoteTitle(int noteId, CancellationToken cancellationToken);

    // additional:

    /// <summary>
    /// Получить страницу каталога.
    /// </summary>
    /// <param name="pageNumber">Номер страницы каталога.</param>
    /// <param name="pageSize">Требуемое количество заметок на странице каталога.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список заметок, представляющий страницу каталога.</returns>
    Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Получить сущность с авторизованным пользователем.
    /// </summary>
    /// <param name="credentialsRequest">Контейнер с данными авторизации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сущность бд с авторизованным пользователем.</returns>
    Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Создать тег если отсутствует.
    /// </summary>
    /// <param name="tag">Именование тега.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    Task CreateTagIfNotExists(string tag, CancellationToken stoppingToken);
}
