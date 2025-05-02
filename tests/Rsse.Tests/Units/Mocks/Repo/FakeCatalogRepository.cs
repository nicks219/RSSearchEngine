using System;
using System.Collections.Generic;
using System.Linq;
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

    private readonly Dictionary<int, Tuple<string, string>> _notes = new();

    public FakeCatalogRepository()
    {
        _notes.Add(TestNoteId, new Tuple<string, string>(FirstNoteTitle, FirstNoteText));
    }

    private int _id;

    public void CreateStubData(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (i != 1 && !_notes.ContainsKey(i))
            {
                _notes.Add(i, new Tuple<string, string>(i + ": key", i + ": value"));
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

    public IQueryable<NoteEntity> ReadAllNotes()
    {
        var notes = _notes
            .Select(keyValue => new NoteEntity
            {
                Text = keyValue.Value.Item2,
                Title = keyValue.Value.Item1,
                NoteId = keyValue.Key
            })
            .ToList();

        return notes.AsQueryable();
    }

    public string ReadNoteTitle(int noteId)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notes.Where(keyValue => keyValue.Key == noteId))
        {
            return keyValue.Value.Item1;
        }

        throw new NotImplementedException($"Note with id `{noteId}` not found");
    }

    public int ReadNoteId(string noteTitle)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notes.Where(keyValue => keyValue.Value.Item1 == noteTitle))
        {
            return keyValue.Key;
        }

        throw new NotImplementedException($"Note `{noteTitle}` not found");
    }

    public Task<int> CreateNote(NoteDto note)
    {
        if (note.TitleRequest == null || note.TextRequest == null || _notes == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notes.Add(_id, new Tuple<string, string>(note.TitleRequest, note.TextRequest));
        _id++;
        return Task.FromResult(_id - 1);
    }

    public Task UpdateCredos(UpdateCredosDto credos) => throw new NotImplementedException();

    public Task<int> DeleteNote(int noteId)
    {
        var res = _notes.Remove(noteId);
        return Task.FromResult(Convert.ToInt32(res));
    }

    //Login Ok
    public Task<UserEntity?> GetUser(LoginDto login)
    {
        var user = login.Password == "skip"
            ? null
            : new UserEntity();

        return Task.FromResult(user);
    }

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize)
    {
        if (_notes == null) throw new NullReferenceException("Data is null");

        var titlesList = pageNumber < 0
            ? throw new Exception("Page number error")
            : Enumerable
                .Range(pageNumber * pageSize, pageSize)
                .Select<int, Tuple<string, int>>(x => new Tuple<string, int>(_notes[x].Item1, x))
                .ToList();

        return new FakeDbSet<Tuple<string, int>>(titlesList);
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

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        var note = new List<Tuple<string, string>> { _notes[noteId] };
        return new FakeDbSet<Tuple<string, string>>(note);
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

    public Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        if (note.TitleRequest == null || note.TextRequest == null || _notes == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notes[note.CommonNoteId] = new Tuple<string, string>(note.TextRequest, note.TitleRequest);

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
