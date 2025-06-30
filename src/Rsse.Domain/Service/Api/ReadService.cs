using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Elector;

namespace SearchEngine.Service.Api;

/// <summary>
/// Функционал чтения заметок.
/// </summary>
public class ReadService(IDataRepository repo)
{
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
        var totalTags = await repo.ReadTags(cancellationToken);
        var enrichedTags = totalTags.Select(t => t.GetEnrichedName()).ToList();

        return new NoteResultDto(enrichedTags);
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору.
    /// </summary>
    /// <param name="request">Данные с отмеченными тегами.</param>
    /// <param name="id">Cтрока с идентификатором, если требуется.</param>
    /// <param name="electionType">Алгоритм выбора следующей заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ с заметкой.</returns>
    public async Task<NoteResultDto> GetNextOrSpecificNote(NoteRequestDto? request, string? id = null,
        ElectionType electionType = ElectionType.SqlRandom, CancellationToken cancellationToken = default)
    {
        var storedTags = await repo.ReadTags(cancellationToken);
        var enrichedTags = storedTags.Select(t => t.GetEnrichedName()).ToList();
        if (request?.CheckedTags == null || storedTags.Count == 0)
        {
            // Если в бд нет тегов, точно стоит отдавать пустой ответ?
            return new NoteResultDto(enrichedTags);
        }

        // Если список отмеченных тегов в запросе пуст, заполняем его полностью.
        var requestCheckedTags = request.CheckedTags.Count == 0
            ? storedTags.Select(t => t.TagId).ToList()
            : request.CheckedTags;

        // Если указан конкретный id, пробуем получить заметку по нему.
        if (int.TryParse(id, out var specificNoteId))
        {
            return await GetNoteOrEmpty(enrichedTags, specificNoteId, cancellationToken);
        }

        // Выбираем заметку по тегам, средствами SQL.
        if (electionType == ElectionType.SqlRandom)
        {
            var noteEntity = await repo.GetRandomNoteOrDefault(requestCheckedTags, cancellationToken);
            return noteEntity == null
                ? new NoteResultDto(enrichedTags)
                : new NoteResultDto(enrichedTags, noteEntity.NoteId, noteEntity.Text, noteEntity.Title);
        }

        // Выбираем заметку по тегам с round robin или rng.
        var electableNoteIds = await repo.ReadTaggedNotesIds(requestCheckedTags, cancellationToken);
        var electedNoteId = NoteElector.ElectNextNote(electableNoteIds, electionType: electionType);

        return await GetNoteOrEmpty(enrichedTags, electedNoteId, cancellationToken);
    }

    private async Task<NoteResultDto> GetNoteOrEmpty(List<string> enrichedTags, int noteId,
        CancellationToken cancellationToken)
    {
        if (noteId < MinIdValue)
        {
            return new NoteResultDto(enrichedTags);
        }

        var note = await repo.ReadNote(noteId, cancellationToken);
        return note == null
            ? new NoteResultDto(enrichedTags)
            : new NoteResultDto(enrichedTags, noteId, note.Text, note.Title);
    }
}
