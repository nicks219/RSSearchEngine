using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;

namespace SearchEngine.Tests.Units.Mocks.Repo;

/// <summary>
/// Тестовый репозиторий.
/// </summary>
public sealed class FakeCatalogRepository : IDataRepository
{
    internal const string FirstNoteText = "Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\n";
    internal const string FirstNoteTitle = "Розенбаум - Вечерняя застольная";
    internal const string SecondNoteText = "Облака, белогривыи лошадки, облака, что ж вы мчитесь?\r\n";
    internal const string SecondNoteTitle = "Шаинский - Облака";
    private const int TestNoteId = 1;

    internal static readonly List<string> TagList = ["Rock", "Pop", "Jazz"];

    private readonly Dictionary<int, TextResultDto> _notes = new();

    public FakeCatalogRepository()
    {
        _notes.Add(TestNoteId, new TextResultDto { Title = FirstNoteTitle, Text = FirstNoteText });
    }

    private int _lastId = 1;

    public void CreateStubData(int count, CancellationToken _)
    {
        for (var i = 0; i < count; i++)
        {
            if (i != 1 && !_notes.ContainsKey(i))
            {
                _notes.Add(i, new TextResultDto { Title = i + ": key", Text = i + ": value" });
            }
        }
    }

    public void RemoveStubData(int count, CancellationToken _)
    {
        for (var i = 0; i < count; i++)
        {
            if (i != 1) _notes.Remove(i);
        }
    }

    /// <inheritdoc/>
    public ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken cancellationToken)
    {
        var notes = _notes
            .Select(keyValue => new NoteEntity
            {
                Text = keyValue.Value.Text,
                Title = keyValue.Value.Title,
                NoteId = keyValue.Key
            })
            .ToList();

        return notes.ToAsyncEnumerable().WithCancellation(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string?> ReadNoteTitle(int noteId, CancellationToken cancellationToken)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notes.Where(keyValue => keyValue.Key == noteId))
        {
            return Task.FromResult<string?>(keyValue.Value.Title);
        }

        return Task.FromResult<string?>(null);
    }

    /// <summary/> Метод для тестового мока, отсутствует в основном контракте
    public int? ReadNoteId(string noteTitle, CancellationToken _)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notes.Where(keyValue => keyValue.Value.Title == noteTitle))
        {
            return keyValue.Key;
        }

        return null;
    }

    /// <inheritdoc/>
    public Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken stoppingToken)
    {
        if (noteRequest.Title == null || noteRequest.Text == null || _notes == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _lastId++;
        _notes.Add(_lastId, new TextResultDto { Title = noteRequest.Title, Text = noteRequest.Text });

        return Task.FromResult(_lastId);
    }

    /// <inheritdoc/>
    public Task UpdateCredos(UpdateCredosRequestDto _, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<int> DeleteNote(int noteId, CancellationToken stoppingToken)
    {
        var res = _notes.Remove(noteId);
        return Task.FromResult(Convert.ToInt32(res));
    }

    /// <inheritdoc/>
    public Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest, CancellationToken cancellationToken)
    {
        var user = credentialsRequest.Password == "skip"
            ? null
            : new UserEntity();

        return Task.FromResult(user);
    }

    /// <inheritdoc/>
    public Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        var titlesList = pageNumber < 0
            ? throw new Exception("Page number error")
            : Enumerable
                .Range(pageNumber * pageSize, pageSize)
                .Select<int, CatalogItemDto>(x => new CatalogItemDto { Title = _notes[x].Title, NoteId = x })
                .ToList();

        return Task.FromResult(titlesList);
    }

    /// <inheritdoc/>
    public Task<List<string>> ReadEnrichedTagList(CancellationToken cancellationToken)
    {
        var result = Task.FromResult(TagList);
        return result;
    }

    /// <inheritdoc/>
    public Task<List<int>> ReadNoteTagIds(int noteId, CancellationToken cancellationToken)
    {
        var tagList = new List<int> { 1, 2 };
        return Task.FromResult(tagList);
    }

    /// <inheritdoc/>
    public Task<int> ReadNotesCount(CancellationToken cancellationToken)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        return Task.FromResult(_notes.Count);
    }

    /// <inheritdoc/>
    public Task<TextResultDto?> ReadNote(int noteId, CancellationToken cancellationToken)
    {
        var note = _notes[noteId];
        return Task.FromResult<TextResultDto?>(note);
    }

    /// <inheritdoc/>
    public Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken cancellationToken)
    {
        var ids = checkedTags as List<int> ?? checkedTags.ToList();
        if (ids.First() == ReadTests.ElectionTestCheckedTag)
        {
            // признак теста ReadManager_Election_ShouldReturnNextNote_OnValidElectionRequest
            // отдаём id тестовой заметки
            ids = [TestNoteId];
        }

        if (ids.Count == ReadTests.ElectionTestTagsCount)
        {
            // признак теста ReadManager_Election_ShouldHasExpectedResponsesDistribution_OnElectionRequests
            // отдаём ElectionTestNotesCount заметок - пусть выбирает
            ids = Enumerable.Range(0, ReadTests.ElectionTestNotesCount).ToList();
        }

        return Task.FromResult(ids);
    }

    /// <inheritdoc/>
    public Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest, CancellationToken stoppingToken)
    {
        if (noteRequest.Title == null || noteRequest.Text == null || _notes == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notes[noteRequest.NoteIdExchange] = new TextResultDto { Text = noteRequest.Text, Title = noteRequest.Title };

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task CreateTagIfNotExists(string _, CancellationToken stoppingToken) => throw new NotImplementedException(nameof(FakeCatalogRepository));
}
