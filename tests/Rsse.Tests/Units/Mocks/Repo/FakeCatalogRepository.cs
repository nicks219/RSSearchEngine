using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore.Testing.Common;
using Moq;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Context;

namespace SearchEngine.Tests.Units.Mocks.Repo;

/// <summary>
/// Тестовый репозиторий
/// </summary>
public class FakeCatalogRepository : IDataRepository
{
    // todo: MySQL WORK. DELETE
    public Task CopyDbFromMysqlToNpgsql() => Task.CompletedTask;
    public BaseCatalogContext? GetReaderContext() => null;
    public BaseCatalogContext? GetPrimaryWriterContext() => null;

    internal const string FirstNoteText = "Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\n";
    internal const string FirstNoteTitle = "Розенбаум - Вечерняя застольная";
    internal const string SecondNoteText = "Облака, белогривыи лошадки, облака, что ж вы мчитесь?\r\n";
    internal const string SecondNoteTitle = "Шаинский - Облака";
    private const int TestNoteId = 1;

    internal static readonly List<string> TagList = ["Rock", "Pop", "Jazz"];

    private readonly Dictionary<int, TextResult> _notes = new();

    public FakeCatalogRepository()
    {
        _notes.Add(TestNoteId, new TextResult { Title = FirstNoteTitle, Text = FirstNoteText });
    }

    private int _lastId = 1;

    public void CreateStubData(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (i != 1 && !_notes.ContainsKey(i))
            {
                _notes.Add(i, new TextResult { Title = i + ": key", Text = i + ": value" });
            }
        }
    }

    public void RemoveStubData(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (i != 1) _notes.Remove(i);
        }
    }

    public IAsyncEnumerable<NoteEntity> ReadAllNotes()
    {
        var notes = _notes
            .Select(keyValue => new NoteEntity
            {
                Text = keyValue.Value.Text,
                Title = keyValue.Value.Title,
                NoteId = keyValue.Key
            })
            .ToList();

        return new FakeDbSet<NoteEntity>(notes);
    }

    public string ReadNoteTitle(int noteId)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notes.Where(keyValue => keyValue.Key == noteId))
        {
            return keyValue.Value.Title;
        }

        throw new NotImplementedException($"Note with id `{noteId}` not found");
    }

    public int ReadNoteId(string noteTitle)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notes.Where(keyValue => keyValue.Value.Title == noteTitle))
        {
            return keyValue.Key;
        }

        throw new NotImplementedException($"Note `{noteTitle}` not found");
    }

    public Task<int> CreateNote(NoteRequestDto noteRequest)
    {
        if (noteRequest.TitleRequest == null || noteRequest.TextRequest == null || _notes == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _lastId++;
        _notes.Add(_lastId, new TextResult { Title = noteRequest.TitleRequest, Text = noteRequest.TextRequest });

        return Task.FromResult(_lastId);
    }

    public Task UpdateCredos(UpdateCredosRequestDto credosRequest) => throw new NotImplementedException();

    public Task<int> DeleteNote(int noteId)
    {
        var res = _notes.Remove(noteId);
        return Task.FromResult(Convert.ToInt32(res));
    }

    //Login Ok
    public Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest)
    {
        var user = credentialsRequest.Password == "skip"
            ? null
            : new UserEntity();

        return Task.FromResult(user);
    }

    public IQueryable<CatalogItemDto> ReadCatalogPage(int pageNumber, int pageSize)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        var titlesList = pageNumber < 0
            ? throw new Exception("Page number error")
            : Enumerable
                .Range(pageNumber * pageSize, pageSize)
                .Select<int, CatalogItemDto>(x => new CatalogItemDto { Title = _notes[x].Title, NoteId = x })
                .ToList();

        return new FakeDbSet<CatalogItemDto>(titlesList);
    }

    public Task<List<string>> ReadStructuredTagList()
    {
        var result = Task.FromResult(TagList);
        return result;
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var tagList = new List<int> { 1, 2 };
        return new FakeDbSet<int>(tagList);
    }

    public Task<int> ReadNotesCount()
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        return Task.FromResult(_notes.Count);
    }

    public IQueryable<TextResult> ReadNote(int noteId)
    {
        var note = new List<TextResult> { _notes[noteId] };
        return new FakeDbSet<TextResult>(note);
    }

    public IQueryable<int> ReadTaggedNotesIds(IEnumerable<int> checkedTags)
    {
        var ids = checkedTags as int[] ?? checkedTags.ToArray();
        if (ids.First() == ReadTests.ElectionTestCheckedTag)
        {
            // признак теста ReadManager_Election_ShouldReturnNextNote_OnValidElectionRequest
            // отдаём id тестовой заметки
            ids = [TestNoteId];
        }

        if (ids.Length == ReadTests.ElectionTestTagsCount)
        {
            // признак теста ReadManager_Election_ShouldHasExpectedResponsesDistribution_OnElectionRequests
            // отдаём ElectionTestNotesCount заметок - пусть выбирает
            ids = Enumerable.Range(0, ReadTests.ElectionTestNotesCount).ToArray();
        }

        // выглядит как архитектурная проблема: IAsyncQueryProvider используется вне слоя репозитория в ElectNextNoteAsync
        var queryable = new List<NoteEntity>().AsQueryable();
        var mock = new Mock<AsyncQueryProvider<NoteEntity>>(queryable) { CallBase = true };
        var asyncQueryProvider = mock.Object;

        var result = new FakeDbSet<int>(ids, asyncQueryProvider);

        return result;
    }

    public Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest)
    {
        if (noteRequest.TitleRequest == null || noteRequest.TextRequest == null || _notes == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notes[noteRequest.NoteIdExchange] = new TextResult { Text = noteRequest.TextRequest, Title = noteRequest.TitleRequest };

        return Task.CompletedTask;
    }

    public Task CreateTagIfNotExists(string tag) => throw new NotImplementedException(nameof(FakeCatalogRepository));

    public void Dispose() => GC.SuppressFinalize(this);

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return new ValueTask();
    }
}
