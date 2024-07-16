using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Elector;

namespace SearchEngine.Models;

/// <summary>
/// Функционал получения заметок
/// </summary>
public class ReadModel(IServiceScope serviceScope)
{
    public const string ElectNoteError = $"[{nameof(ReadModel)}: {nameof(GetNextOrSpecificNote)} error]";
    private const string ReadTitleByNoteIdError = $"[{nameof(ReadModel)}: {nameof(ReadTitleByNoteId)} error]";
    private const string ReadTagListError = $"[{nameof(ReadModel)}: {nameof(ReadTagList)} error]";

    private readonly ILogger<ReadModel> _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ReadModel>>();
    private readonly IDataRepository _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();

    /// <summary>
    /// Прочитать название заметки по её идентификатору
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <returns>название заметки</returns>
    public string? ReadTitleByNoteId(int id)
    {
        try
        {
            var res = _repo.ReadNoteTitle(id);

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTitleByNoteIdError);

            return null;
        }
    }

    /// <summary>
    /// Получить структурированный список тегов
    /// </summary>
    /// <returns>ответ со списком тегов</returns>
    public async Task<NoteDto> ReadTagList()
    {
        try
        {
            var tagList = await _repo.ReadStructuredTagList();

            return new NoteDto(tagList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTagListError);

            return new NoteDto { CommonErrorMessageResponse = ReadTagListError };
        }
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору
    /// </summary>
    /// <param name="request">данные с отмеченными тегами</param>
    /// <param name="id">строка с идентификатором, если требуется</param>
    /// <param name="randomElection">алгоритм выбора следующей заметки</param>
    /// <returns>ответ с заметкой</returns>
    public async Task<NoteDto> GetNextOrSpecificNote(NoteDto? request, string? id = null, bool randomElection = true)
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
                    noteId = await _repo.ElectNextNote(request.TagsCheckedRequest, randomElection);
                }

                if (noteId != 0)
                {
                    var notes = await _repo
                        .ReadNote(noteId)
                        .ToListAsync();

                    if (notes.Count > 0)
                    {
                        text = notes[0].Item1;

                        title = notes[0].Item2;
                    }
                }
            }

            var tagList = await _repo.ReadStructuredTagList();

            return new NoteDto(tagList, noteId, text, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ElectNoteError);

            return new NoteDto { CommonErrorMessageResponse = ElectNoteError };
        }

        bool IsSpecific() => int.TryParse(id, out noteId);
    }
}
