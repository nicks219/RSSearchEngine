using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using static SearchEngine.Service.Configuration.ServiceErrorMessages;

namespace SearchEngine.Services;

/// <summary>
/// Функционал создания заметок.
/// </summary>
public class CreateService(IDataRepository repo, ILogger<CreateService> logger)
{
    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);

    /// <summary>
    /// Создать заметку.
    /// </summary>
    /// <param name="noteRequestDto">Данные для создания заметки.</param>
    /// <returns>Контейнер с иснформации о созданной заметке.</returns>
    public async Task<NoteResultDto> CreateNote(NoteRequestDto noteRequestDto)
    {
        try
        {
            var enrichedTags = await repo.ReadEnrichedTagList();
            var unsuccessfulResultDto = new NoteResultDto(enrichedTags: enrichedTags);

            if (noteRequestDto.CheckedTags == null ||
                string.IsNullOrEmpty(noteRequestDto.Text) ||
                string.IsNullOrEmpty(noteRequestDto.Title) ||
                noteRequestDto.CheckedTags.Count == 0)
            {
                // пользовательская ошибка: невалидные данные
                if (string.IsNullOrEmpty(noteRequestDto.Text))
                {
                    return unsuccessfulResultDto with { ErrorMessage = CreateNoteEmptyDataError };
                }

                unsuccessfulResultDto = unsuccessfulResultDto with
                {
                    ErrorMessage = CreateNoteEmptyDataError,
                    Text = noteRequestDto.Text,
                    Title = noteRequestDto.Title
                };

                return unsuccessfulResultDto;
            }

            noteRequestDto = noteRequestDto with { Title = noteRequestDto.Title };

            var newNoteId = await repo.CreateNote(noteRequestDto);

            if (newNoteId == 0)
            {
                // пользовательская ошибка: не создалась заметка
                unsuccessfulResultDto = unsuccessfulResultDto with
                {
                    ErrorMessage = CreateNoteUnsuccessfulError,
                    Title = "[Already Exist]"
                };

                return unsuccessfulResultDto;
            }

            var tagsAfterCreate = await repo.ReadEnrichedTagList();
            var totalTagsCount = tagsAfterCreate.Count;
            var noteTagIds = await repo.ReadNoteTagIds(newNoteId);

            var checkboxes = TagConverter.AllToFlags(noteTagIds, totalTagsCount);

            return new NoteResultDto(tagsAfterCreate, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateNoteError);

            return new NoteResultDto { ErrorMessage = CreateNoteError };
        }
    }

    /// <summary>
    /// Создать новый тег из размеченного квадратными скобками заголовка.
    /// </summary>
    /// <param name="noteDto">Данные для создания тега.</param>
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
}
