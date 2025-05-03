using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал создания заметок
/// </summary>
public class CreateManager(IServiceProvider scopedProvider)
{
    private readonly IDataRepository _repo = scopedProvider.GetRequiredService<IDataRepository>();
    private readonly ILogger<CreateManager> _logger = scopedProvider.GetRequiredService<ILogger<CreateManager>>();

    /// <summary>
    /// Получить структурированный список тегов
    /// </summary>
    /// <returns>шаблон с ответом</returns>
    public async Task<NoteResultDto> ReadStructuredTagList()
    {
        try
        {
            var tagList = await _repo.ReadStructuredTagList();

            return new NoteResultDto(tagList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateManagerReadTagListError);
            return new NoteResultDto { CommonErrorMessageResponse = CreateManagerReadTagListError };
        }
    }

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="noteRequestDto">данные для создания заметки</param>
    /// <returns>созданная заметка</returns>
    public async Task<NoteResultDto> CreateNote(NoteRequestDto noteRequestDto)
    {
        try
        {
            if (noteRequestDto.TagsCheckedRequest == null ||
                string.IsNullOrEmpty(noteRequestDto.TextRequest) ||
                string.IsNullOrEmpty(noteRequestDto.TitleRequest) ||
                noteRequestDto.TagsCheckedRequest.Count == 0)
            {
                var errorDtoWithTags = await ReadStructuredTagList();

                errorDtoWithTags.CommonErrorMessageResponse = CreateNoteEmptyDataError;

                if (string.IsNullOrEmpty(noteRequestDto.TextRequest))
                {
                    return errorDtoWithTags;
                }

                errorDtoWithTags.TextResponse = noteRequestDto.TextRequest;
                errorDtoWithTags.TitleResponse = noteRequestDto.TitleRequest;

                return errorDtoWithTags;
            }

            // createdNote.Text =  Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(createdNote.Text)).ToString();

            noteRequestDto.TitleRequest = noteRequestDto.TitleRequest.Trim();

            var newNoteId = await _repo.CreateNote(noteRequestDto);

            if (newNoteId == 0)
            {
                var errorDtoWithTags = await ReadStructuredTagList();

                errorDtoWithTags.CommonErrorMessageResponse = CreateNoteUnsuccessfulError;

                errorDtoWithTags.TitleResponse = "[Already Exist]";

                return errorDtoWithTags;
            }

            var updatedDto = await ReadStructuredTagList();

            var updatedTagList = updatedDto.StructuredTagsListResponse!;

            var checkboxes = new List<string>();

            for (var i = 0; i < updatedTagList.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in noteRequestDto.TagsCheckedRequest)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteResultDto(updatedTagList, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateNoteError);

            return new NoteResultDto { CommonErrorMessageResponse = CreateNoteError };
        }
    }

    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);

    /// <summary>
    /// Создать новый тег из размеченного квадратными скобками заголовка
    /// </summary>
    /// <param name="noteDto">данные для создания тега</param>
    internal Task CreateTagFromTitle(NoteRequestDto? noteDto)
    {
        const string tagPattern = "[]";

        if (noteDto?.TitleRequest == null)
        {
            return Task.CompletedTask;
        }

        var tag = TitlePattern.Match(noteDto.TitleRequest).Value.Trim(tagPattern.ToCharArray());

        return !string.IsNullOrEmpty(tag)
            ? _repo.CreateTagIfNotExists(tag)
            : Task.CompletedTask;
    }
}
