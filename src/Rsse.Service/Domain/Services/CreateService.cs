using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Services;

/// <summary>
/// Функционал создания заметок
/// </summary>
public class CreateService(IDataRepository repo, ILogger<CreateService> logger)
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

            if (noteRequestDto.CheckedTags == null ||
                string.IsNullOrEmpty(noteRequestDto.Text) ||
                string.IsNullOrEmpty(noteRequestDto.Title) ||
                noteRequestDto.CheckedTags.Count == 0)
            {
                // пользовательская ошибка: невалидные данные
                if (string.IsNullOrEmpty(noteRequestDto.Text))
                {
                    return dtoWithInitialTags with { ErrorMessage = CreateNoteEmptyDataError };
                }

                dtoWithInitialTags = dtoWithInitialTags with
                {
                    ErrorMessage = CreateNoteEmptyDataError,
                    Text = noteRequestDto.Text,
                    Title = noteRequestDto.Title
                };

                return dtoWithInitialTags;
            }

            // todo: перенести в маппер
            noteRequestDto = noteRequestDto with { Title = noteRequestDto.Title.Trim() };

            var newNoteId = await repo.CreateNote(noteRequestDto);

            if (newNoteId == 0)
            {
                // пользовательская ошибка: не создалась заметка
                dtoWithInitialTags = dtoWithInitialTags with
                {
                    ErrorMessage = CreateNoteUnsuccessfulError,
                    Title = "[Already Exist]"
                };

                return dtoWithInitialTags;
            }

            var dtoWithTagsAfterSuccessfulCreate = await ReadStructuredTagList();

            var tagsAfterCreate = dtoWithTagsAfterSuccessfulCreate.StructuredTags;

            var checkboxes = new List<string>();

            for (var i = 0; i < tagsAfterCreate!.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in noteRequestDto.CheckedTags)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteResultDto(tagsAfterCreate, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateNoteError);

            return new NoteResultDto { ErrorMessage = CreateNoteError };
        }
    }

    /// <summary>
    /// Создать новый тег из размеченного квадратными скобками заголовка
    /// </summary>
    /// <param name="noteDto">данные для создания тега</param>
    internal async Task CreateTagFromTitle(NoteRequestDto? noteDto)
    {
        const string tagPattern = "[]";

        if (noteDto?.Title == null)
        {
            return;
        }

        var tag = TitlePattern.Match(noteDto.Title).Value.Trim(tagPattern.ToCharArray());

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
            return new NoteResultDto { ErrorMessage = CreateManagerReadTagListError };
        }
    }

}
