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
    public async Task<NoteDto> ReadStructuredTagList()
    {
        try
        {
            var tagList = await _repo.ReadStructuredTagList();

            return new NoteDto(tagList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateManagerReadTagListError);
            return new NoteDto { CommonErrorMessageResponse = CreateManagerReadTagListError };
        }
    }

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="noteDto">данные для создания заметки</param>
    /// <returns>созданная заметка</returns>
    public async Task<NoteDto> CreateNote(NoteDto noteDto)
    {
        try
        {
            if (noteDto.TagsCheckedRequest == null ||
                string.IsNullOrEmpty(noteDto.TextRequest) ||
                string.IsNullOrEmpty(noteDto.TitleRequest) ||
                noteDto.TagsCheckedRequest.Count == 0)
            {
                var errorDto = await ReadStructuredTagList();

                errorDto.CommonErrorMessageResponse = CreateNoteEmptyDataError;

                if (string.IsNullOrEmpty(noteDto.TextRequest))
                {
                    return errorDto;
                }

                errorDto.TextResponse = noteDto.TextRequest;
                errorDto.TitleResponse = noteDto.TitleRequest;

                return errorDto;
            }

            // createdNote.Text =  Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(createdNote.Text)).ToString();

            noteDto.TitleRequest = noteDto.TitleRequest.Trim();

            var newNoteId = await _repo.CreateNote(noteDto);

            if (newNoteId == 0)
            {
                var errorDto = await ReadStructuredTagList();

                errorDto.CommonErrorMessageResponse = CreateNoteUnsuccessfulError;

                errorDto.TitleResponse = "[Already Exist]";

                return errorDto;
            }

            var updatedDto = await ReadStructuredTagList();

            var updatedTagList = updatedDto.StructuredTagsListResponse!;

            var checkboxes = new List<string>();

            for (var i = 0; i < updatedTagList.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in noteDto.TagsCheckedRequest)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteDto(updatedTagList, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateNoteError);

            return new NoteDto { CommonErrorMessageResponse = CreateNoteError };
        }
    }

    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);

    /// <summary>
    /// Создать новый тег из размеченного квадратными скобками заголовка
    /// </summary>
    /// <param name="noteDto">данные для создания тега</param>
    internal Task CreateTagFromTitle(NoteDto? noteDto)
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
