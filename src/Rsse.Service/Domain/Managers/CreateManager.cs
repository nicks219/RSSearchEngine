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
    public async Task<NoteResultDto> CreateNote(NoteRequestDto noteRequestDto)
    {
        try
        {
            var dtoWithInitialTags = await ReadStructuredTagList();

            if (noteRequestDto.TagsCheckedRequest == null ||
                string.IsNullOrEmpty(noteRequestDto.TextRequest) ||
                string.IsNullOrEmpty(noteRequestDto.TitleRequest) ||
                noteRequestDto.TagsCheckedRequest.Count == 0)
            {
                // пользовательская ошибка: невалидные данные
                if (string.IsNullOrEmpty(noteRequestDto.TextRequest))
                {
                    return dtoWithInitialTags with { CommonErrorMessageResponse = CreateNoteEmptyDataError };
                }

                dtoWithInitialTags = dtoWithInitialTags with
                {
                    CommonErrorMessageResponse = CreateNoteEmptyDataError,
                    TextResponse = noteRequestDto.TextRequest,
                    TitleResponse = noteRequestDto.TitleRequest
                };

                return dtoWithInitialTags;
            }

            // todo: перенести в маппер
            noteRequestDto = noteRequestDto with { TitleRequest = noteRequestDto.TitleRequest.Trim() };

            var newNoteId = await repo.CreateNote(noteRequestDto);

            if (newNoteId == 0)
            {
                // пользовательская ошибка: не создалась заметка
                dtoWithInitialTags = dtoWithInitialTags with
                {
                    CommonErrorMessageResponse = CreateNoteUnsuccessfulError,
                    TitleResponse = "[Already Exist]"
                };

                return dtoWithInitialTags;
            }

            var dtoWithTagsAfterSuccessfulCreate = await ReadStructuredTagList();

            var tagsAfterCreate = dtoWithTagsAfterSuccessfulCreate.StructuredTagsListResponse;

            var checkboxes = new List<string>();

            for (var i = 0; i < tagsAfterCreate!.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in noteRequestDto.TagsCheckedRequest)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteResultDto(tagsAfterCreate, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateNoteError);

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
