using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Service.Models;

public class CreateModel
{
    private const string ReadTagListError = $"[{nameof(CreateModel)}: {nameof(ReadTagList)} error]";
    private const string CreateNoteError = $"[{nameof(CreateModel)}: {nameof(CreateNote)} error]";
    private const string CreateNoteUnsuccessfulError = $"[{nameof(CreateModel)}: {nameof(CreateNote)} error: create unsuccessful]";
    private const string CreateNoteEmptyDataError = $"[{nameof(CreateModel)}: {nameof(CreateNote)} error: empty data]";

    private readonly IDataRepository _repo;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IServiceScope serviceScope)
    {
        _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
        _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<CreateModel>>();
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
            return new NoteDto { ErrorMessageResponse = ReadTagListError };
        }
    }

    public async Task<NoteDto> CreateNote(NoteDto createdNote)
    {
        try
        {
            if (createdNote.SongGenres == null ||
                string.IsNullOrEmpty(createdNote.Text) ||
                string.IsNullOrEmpty(createdNote.Title) ||
                createdNote.SongGenres.Count == 0)
            {
                var errorDto = await ReadTagList();

                errorDto.ErrorMessageResponse = CreateNoteEmptyDataError;

                if (string.IsNullOrEmpty(createdNote.Text))
                {
                    return errorDto;
                }

                errorDto.TextResponse = createdNote.Text;
                errorDto.TitleResponse = createdNote.Title;

                return errorDto;
            }

            //createdNote.Text =  Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(createdNote.Text)).ToString();

            createdNote.Title = createdNote.Title.Trim();

            var newNoteId = await _repo.CreateNote(createdNote);

            if (newNoteId == 0)
            {
                var errorDto = await ReadTagList();

                errorDto.ErrorMessageResponse = CreateNoteUnsuccessfulError;

                errorDto.TitleResponse = "[Already Exist]";

                return errorDto;
            }

            var updatedDto = await ReadTagList();

            var updatedTagList = updatedDto.GenreListResponse!;

            var checkboxes = new List<string>();

            for (var i = 0; i < updatedTagList.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in createdNote.SongGenres)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteDto(updatedTagList, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateNoteError);

            return new NoteDto { ErrorMessageResponse = CreateNoteError };
        }
    }

    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);

    public Task CreateTag(NoteDto? dto)
    {
        if (dto?.Title == null)
        {
            return Task.CompletedTask;
        }

        var tag = TitlePattern.Match(dto.Title).Value.Trim("[]".ToCharArray());

        return !string.IsNullOrEmpty(tag)
            ? _repo.CreateTagIfNotExists(tag)
            : Task.CompletedTask;
    }
}
