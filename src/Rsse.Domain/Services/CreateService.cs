
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using static SearchEngine.Service.Configuration.ServiceErrorMessages;

namespace SearchEngine.Services;

/// <summary>
/// Функционал создания заметок.
/// </summary>
public partial class CreateService(IDataRepository repo)
{
    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = TitleRegex();

    /// <summary>
    /// Создать заметку.
    /// </summary>
    /// <param name="noteRequestDto">Данные для создания заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Контейнер с иснформации о созданной заметке.</returns>
    public async Task<NoteResultDto> CreateNote(NoteRequestDto noteRequestDto, CancellationToken stoppingToken)
    {
        var enrichedTags = await repo.ReadEnrichedTagList(stoppingToken);
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

        var newNoteId = await repo.CreateNote(noteRequestDto, stoppingToken);

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

        var tagsAfterCreate = await repo.ReadEnrichedTagList(stoppingToken);
        var totalTagsCount = tagsAfterCreate.Count;
        var noteTagIds = await repo.ReadNoteTagIds(newNoteId, stoppingToken);

        var checkboxes = TagConverter.AllToFlags(noteTagIds, totalTagsCount);

        return new NoteResultDto(tagsAfterCreate, newNoteId, "", "[OK]", checkboxes);
    }

    /// <summary>
    /// Создать новый тег из размеченного квадратными скобками заголовка.
    /// </summary>
    /// <param name="noteDto">Данные для создания тега.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    internal async Task CreateTagFromTitle(NoteRequestDto? noteDto, CancellationToken stoppingToken)
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

        await repo.CreateTagIfNotExists(tag, stoppingToken);
    }
    [GeneratedRegex(@"\[(.+?)\]", RegexOptions.Compiled)]
    private static partial Regex TitleRegex();
}
