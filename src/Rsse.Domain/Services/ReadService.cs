using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Elector;

namespace SearchEngine.Services;

/// <summary>
/// Функционал чтения заметок.
/// </summary>
public class ReadService(IDataRepository repo)
{
    /// <summary>
    /// Кэшируем диапазон идентификаторов тегов тк сами теги добавляются редко.
    /// В данной реализации кэш будет обновлён только при перезапуске сервиса.
    /// </summary>
    private static List<int>? _allTagsRange;

    /// <summary>
    /// Минимальное значение идентфикатора в бд.
    /// </summary>
    private const int MinIdValue = 1;

    /// <summary>
    /// Прочитать название заметки по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Название заметки.</returns>
    public async Task<string?> ReadTitleByNoteId(int id, CancellationToken cancellationToken)
    {
        var res = await repo.ReadNoteTitle(id, cancellationToken);

        return res;
    }

    /// <summary>
    /// Получить список тегов, обогащенный количеством заметок по каждому тегу.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ со списком обогащенных тегов.</returns>
    public async Task<NoteResultDto> ReadEnrichedTagList(CancellationToken cancellationToken)
    {
        var enrichedTagList = await repo.ReadEnrichedTagList(cancellationToken);

        return new NoteResultDto(enrichedTagList);
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору.
    /// </summary>
    /// <param name="request">Данные с отмеченными тегами.</param>
    /// <param name="id">Cтрока с идентификатором, если требуется.</param>
    /// <param name="randomElectionEnabled">Алгоритм выбора следующей заметки, <b>true</b> случайный выбор.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ с заметкой.</returns>
    public async Task<NoteResultDto> GetNextOrSpecificNote(NoteRequestDto? request, string? id = null,
        bool randomElectionEnabled = true, CancellationToken cancellationToken = default)
    {
        var totalTags = await repo.ReadEnrichedTagList(cancellationToken);
        if (request?.CheckedTags == null || totalTags.Count == 0)
        {
            return new NoteResultDto(totalTags);
        }

        // Если список тегов пуст, заполняем его полностью.
        if (request.CheckedTags.Count == 0)
        {
            _allTagsRange ??= Enumerable.Range(AppConstants.MinTagNumber, totalTags.Count).ToList();
            request = request with { CheckedTags = _allTagsRange };
        }

        // Если указан конкретный id, пробуем получить заметку по нему.
        if (int.TryParse(id, out var specificNoteId))
        {
            return await GetNoteOrEmpty(totalTags, specificNoteId, cancellationToken);
        }

        // Выбираем заметку по тегам, средствами SQL.
        if (randomElectionEnabled)
        {
            var noteEntity = await repo.GetRandomNoteOrDefault(request.CheckedTags, cancellationToken);
            return noteEntity == null
                ? new NoteResultDto(totalTags)
                : new NoteResultDto(totalTags, noteEntity.NoteId, noteEntity.Text!, noteEntity.Title!);
        }

        // Выбираем заметку по тегам с round robin.
        var electableNoteIds = await repo.ReadTaggedNotesIds(request.CheckedTags, cancellationToken);
        var electedNoteId = NoteElector.ElectNextNote(electableNoteIds, randomElectionEnabled: false);

        return await GetNoteOrEmpty(totalTags, electedNoteId, cancellationToken);
    }

    private async Task<NoteResultDto> GetNoteOrEmpty(List<string> totalTags, int noteId,
        CancellationToken cancellationToken)
    {
        if (noteId < MinIdValue)
        {
            return new NoteResultDto(totalTags);
        }

        var note = await repo.ReadNote(noteId, cancellationToken);
        return note == null
            ? new NoteResultDto(totalTags)
            : new NoteResultDto(totalTags, noteId, note.Text, note.Title);
    }
}
