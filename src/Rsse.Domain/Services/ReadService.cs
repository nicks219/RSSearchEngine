using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Elector;

namespace SearchEngine.Services;

/// <summary>
/// Функционал чтения заметок.
/// </summary>
public class ReadService(IDataRepository repo)
{
    /// <summary>
    /// Прочитать название заметки по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <returns>Название заметки.</returns>
    public async Task<string?> ReadTitleByNoteId(int id)
    {
        var res = await repo.ReadNoteTitle(id);

        return res;
    }

    /// <summary>
    /// Получить список тегов, обогащенный количеством заметок по каждому тегу.
    /// </summary>
    /// <returns>Ответ со списком обогащенных тегов.</returns>
    public async Task<NoteResultDto> ReadEnrichedTagList()
    {
        var enrichedTagList = await repo.ReadEnrichedTagList();

        return new NoteResultDto(enrichedTagList);
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору.
    /// </summary>
    /// <param name="request">Данные с отмеченными тегами.</param>
    /// <param name="id">Cтрока с идентификатором, если требуется.</param>
    /// <param name="randomElectionEnabled">Алгоритм выбора следующей заметки, <b>true</b> случайный выбор.</param>
    /// <returns>Ответ с заметкой.</returns>
    public async Task<NoteResultDto> GetNextOrSpecificNote(NoteRequestDto? request, string? id = null,
        bool randomElectionEnabled = true)
    {
        if (request?.CheckedTags?.Count == 0)
        {
            // Для пустого запроса считаем все теги отмеченными.
            var checkedTags = Enumerable.Range(1, 44).ToList();
            request = request with { CheckedTags = checkedTags };
        }

        var text = string.Empty;
        var title = string.Empty;
        var noteId = 0;

        if (request is { CheckedTags: not null } && request.CheckedTags.Count != 0)
        {
            if (IsSpecific() == false)
            {
                var checkedTags = request.CheckedTags;
                // todo: вычитывается весь список
                var electableNoteIds = await repo.ReadTaggedNotesIds(checkedTags);
                noteId = NoteElector.ElectNextNote(electableNoteIds, randomElectionEnabled);
            }

            if (noteId != 0)
            {
                var note = await repo.ReadNote(noteId);

                if (note != null)
                {
                    text = note.Text;

                    title = note.Title;
                }
            }
        }

        var tagList = await repo.ReadEnrichedTagList();

        return new NoteResultDto(tagList, noteId, text, title);

        bool IsSpecific() => int.TryParse(id, out noteId);
    }
}
