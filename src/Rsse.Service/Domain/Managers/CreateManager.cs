using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал создания заметок
/// </summary>
public class CreateManager(IDataRepository repo, ILogger<CreateManager> logger)
{
    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="noteRequestDto">данные для создания заметки</param>
    /// <returns>созданная заметка</returns>
    public async Task<NoteBaseResultDto> CreateNote(NoteRequestDto noteRequestDto)
    {
        try
        {
            if (noteRequestDto.TagsCheckedRequest == null ||
                string.IsNullOrEmpty(noteRequestDto.TextRequest) ||
                string.IsNullOrEmpty(noteRequestDto.TitleRequest) ||
                noteRequestDto.TagsCheckedRequest.Count == 0)
            {
                // невалидные данные из запроса
                var dtoWithTags = await ReadStructuredTagList();

                var errorDtoWithTags = new NoteErrorResultDto
                {
                        StructuredTagsListResponse = dtoWithTags.StructuredTagsListResponse,
                        CommonErrorMessageResponse = CreateNoteEmptyDataError
                };

                if (string.IsNullOrEmpty(noteRequestDto.TextRequest))
                {
                    return errorDtoWithTags;
                }

                errorDtoWithTags = errorDtoWithTags with
                {
                    TextResponse = noteRequestDto.TextRequest,
                    TitleResponse = noteRequestDto.TitleRequest
                };

                return errorDtoWithTags;
            }

            // createdNote.Text =  Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(createdNote.Text)).ToString();

            noteRequestDto = noteRequestDto with { TitleRequest = noteRequestDto.TitleRequest.Trim() };

            var newNoteId = await repo.CreateNote(noteRequestDto);

            if (newNoteId == 0)
            {
                // не создалась заметка
                var dtoWithTags = await ReadStructuredTagList();

                var errorDtoWithTags = new NoteErrorResultDto
                {
                    StructuredTagsListResponse = dtoWithTags.StructuredTagsListResponse,
                    CommonErrorMessageResponse = CreateNoteUnsuccessfulError,
                    TitleResponse = "[Already Exist]"
                };

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
            logger.LogError(ex, CreateNoteError);

            // системная ошибка
            return new NoteResultDto { CommonErrorMessageResponse = CreateNoteError };
        }
    }

    /// <summary>
    /// Создать новый тег из размеченного квадратными скобками заголовка
    /// </summary>
    /// <param name="noteDto">данные для создания тега</param>
    internal async Task CreateTagFromTitle(NoteRequestDto? noteDto)
    {
        const string tagPattern = "[]";

        if (noteDto?.TitleRequest == null)
        {
            return;
        }

        var tag = TitlePattern.Match(noteDto.TitleRequest).Value.Trim(tagPattern.ToCharArray());

        if (string.IsNullOrEmpty(tag))
        {
            return;
        }

        await repo.CreateTagIfNotExists(tag);
    }

    /// <summary>
    /// Получить структурированный список тегов
    /// </summary>
    /// <returns>шаблон с ответом</returns>
    internal async Task<NoteResultDto> ReadStructuredTagList()
    {
        try
        {
            var tagList = await repo.ReadStructuredTagList();

            return new NoteResultDto(tagList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateManagerReadTagListError);
            return new NoteResultDto { CommonErrorMessageResponse = CreateManagerReadTagListError };
        }
    }

}
