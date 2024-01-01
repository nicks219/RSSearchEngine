using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Engine;

namespace SearchEngine.Service.Models;

public class ReadModel
{
    public const string ElectNoteError = $"[{nameof(ReadModel)}: {nameof(ElectNote)} error]";
    private const string ReadTitleByNoteIdError = $"[{nameof(ReadModel)}: {nameof(ReadTitleByNoteId)} error]";
    private const string ReadTagListError = $"[{nameof(ReadModel)}: {nameof(ReadTagList)} error]";

    private readonly ILogger<ReadModel> _logger;
    private readonly IDataRepository _repo;

    public ReadModel(IServiceScope serviceScope)
    {
        _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ReadModel>>();
        _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
    }

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

    public async Task<NoteDto> ReadTagList()
    {
        try
        {
            var tagList = await _repo.ReadGeneralTagList();

            return new NoteDto(tagList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTagListError);

            return new NoteDto { CommonErrorMessageResponse = ReadTagListError };
        }
    }

    public async Task<NoteDto> ElectNote(NoteDto? request, string? id = null, bool randomElection = true)
    {
        var text = string.Empty;
        var title = string.Empty;
        var noteId = 0;

        try
        {
            if (request is { TagsCheckedRequest: not null } && request.TagsCheckedRequest.Count != 0)
            {
                if (!int.TryParse(id, out noteId))
                {
                    noteId = await _repo.GetElectedNoteId(request.TagsCheckedRequest, randomElection);
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

            var tagList = await _repo.ReadGeneralTagList();

            return new NoteDto(tagList, noteId, text, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ElectNoteError);

            return new NoteDto { CommonErrorMessageResponse = ElectNoteError };
        }
    }
}
