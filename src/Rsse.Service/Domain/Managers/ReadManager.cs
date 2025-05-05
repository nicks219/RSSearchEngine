using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Elector;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал получения заметок
/// </summary>
public class ReadManager(IDataRepository repo, ILogger<ReadManager> logger)
{
    /// <summary>
    /// Прочитать название заметки по её идентификатору
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <returns>название заметки</returns>
    public string? ReadTitleByNoteId(int id)
    {
        try
        {
            var res = repo.ReadNoteTitle(id);

            return res;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTitleByNoteIdError);

            return null;
        }
    }

    /// <summary>
    /// Получить структурированный список тегов
    /// </summary>
    /// <returns>ответ со списком тегов</returns>
    public async Task<NoteResultDto> ReadTagList()
    {
        try
        {
            var tagList = await repo.ReadStructuredTagList();

            return new NoteResultDto(tagList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadModelReadTagListError);

            return new NoteResultDto { CommonErrorMessageResponse = ReadModelReadTagListError };
        }
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору
    /// </summary>
    /// <param name="request">данные с отмеченными тегами</param>
    /// <param name="id">строка с идентификатором, если требуется</param>
    /// <param name="randomElectionEnabled">алгоритм выбора следующей заметки</param>
    /// <returns>ответ с заметкой</returns>
    public async Task<NoteResultDto> GetNextOrSpecificNote(NoteRequestDto? request, string? id = null, bool randomElectionEnabled = true)
    {
        var text = string.Empty;
        var title = string.Empty;
        var noteId = 0;

        try
        {
            if (request is { TagsCheckedRequest: not null } && request.TagsCheckedRequest.Count != 0)
            {
                if (IsSpecific() == false)
                {
                    var checkedTags = request.TagsCheckedRequest;
                    var electableNoteIds = repo.ReadTaggedNotesIds(checkedTags);
                    noteId = await NoteElector.ElectNextNoteAsync(electableNoteIds, randomElectionEnabled);
                }

                if (noteId != 0)
                {
                    var notes = await repo
                        .ReadNote(noteId)
                        .ToListAsync();

                    if (notes.Count > 0)
                    {
                        text = notes[0].Text;

                        title = notes[0].Title;
                    }
                }
            }

            var tagList = await repo.ReadStructuredTagList();

            return new NoteResultDto(tagList, noteId, text, title);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ElectNoteError);

            return new NoteResultDto { CommonErrorMessageResponse = ElectNoteError };
        }

        bool IsSpecific() => int.TryParse(id, out noteId);
    }
}
