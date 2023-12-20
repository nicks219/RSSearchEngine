using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Tests.Infrastructure.DAL;

public class TestDataRepository : IDataRepository
{
    internal const string FirstNoteText = "Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\n";
    internal const string FirstNoteTitle = "Розенбаум - Вечерняя застольная";
    internal const string SecondNoteText = "Облака, белогривыи лошадки, облака, что ж вы мчитесь?\r\n";
    internal const string SecondNoteTitle = "Шаинский - Облака";

    internal static readonly List<string> TagList = new() { "Rock", "Pop", "Jazz" };
    // избавляйся от статики, даже в тестовом моке:
    private static Dictionary<int, Tuple<string, string>>? _notesTableStub = new()
    {
        { 1, new Tuple<string, string>(FirstNoteTitle, FirstNoteText)}
    };

    private int _id;

    public static void CreateStubData(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (_notesTableStub?.ContainsKey(i) == false)
            {
                _notesTableStub.Add(i, new Tuple<string, string>(i + ": key", i + ": value"));
            }
        }
    }

    public static void RemoveStubData(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (_notesTableStub?.ContainsKey(i) == true)
            {
                _notesTableStub.Remove(i);
            }
        }
    }

    public IQueryable<TextEntity> ReadAllNotes()
    {
        var songs = _notesTableStub?
            .Select(entity => new TextEntity
            {
                Song = entity.Value.Item2,
                Title = entity.Value.Item1,
                TextId = entity.Key
            })
            .ToList();

        return new TestQueryable<TextEntity>(songs!);
    }

    public string ReadTitleByNoteId(int id)
    {
        if (_notesTableStub == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notesTableStub.Where(keyValue => keyValue.Key == id))
        {
            return keyValue.Value.Item1;
        }

        throw new NotImplementedException($"Note with id `{id}` not found");
    }

    public int FindNoteIdByTitle(string noteTitle)
    {
        if (_notesTableStub == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notesTableStub.Where(keyValue => keyValue.Value.Item1 == noteTitle))
        {
            return keyValue.Key;
        }

        throw new NotImplementedException($"Note `{noteTitle}` not found");
    }

    public Task<int> CreateNote(NoteDto? dt)
    {
        if (dt?.Title == null || dt.Text == null || _notesTableStub == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notesTableStub.Add(_id, new Tuple<string, string>(dt.Title, dt.Text));
        _id++;
        return Task.FromResult(_id - 1);
    }

    public Task<int> DeleteNote(int noteId)
    {
        var res = _notesTableStub?.Remove(noteId);
        return Task.FromResult(Convert.ToInt32(res));
    }

    //Login Ok
    public Task<UserEntity?> GetUser(LoginDto dt)
    {
        var user = dt.Password == "skip"
            ? null
            : new UserEntity();

        return Task.FromResult(user);
    }

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize)
    {
        if (_notesTableStub == null) throw new NullReferenceException("Data is null");

        var titlesList = pageNumber < 0
            ? throw new Exception("Page number error")
            : Enumerable
                .Range(pageNumber * pageSize, pageSize)
                .Select<int, Tuple<string, int>>(x => new Tuple<string, int>(_notesTableStub[x].Item1, x))
                .ToList();

        return new TestQueryable<Tuple<string, int>>(titlesList);
    }

    public Task<List<string>> ReadGeneralTagList()
    {
        var result = Task.FromResult(TagList);
        return result;
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var tagList = new List<int> { 1, 2 };
        return new TestQueryable<int>(tagList);
    }

    public Task<int> ReadNotesCount()
    {
        if (_notesTableStub == null) throw new NullReferenceException("Data is null");

        return Task.FromResult(_notesTableStub.Count);
    }

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        _notesTableStub ??= new Dictionary<int, Tuple<string, string>> { { 1, new Tuple<string, string>(FirstNoteTitle, FirstNoteText) } };

        return new TestQueryable<Tuple<string, string>>(
            new List<Tuple<string, string>>
            {
                _notesTableStub[noteId]
            });
    }

    public IQueryable<int> ReadAllNotesTaggedBy(IEnumerable<int> checkedTags)
    {
        return new TestQueryable<int>(checkedTags);
    }

    public Task UpdateNote(IEnumerable<int> initialTags, NoteDto? dt)
    {
        if (dt?.Title == null || dt.Text == null || _notesTableStub == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notesTableStub[dt.Id] = new Tuple<string, string>(dt.Title, dt.Text);

        return Task.CompletedTask;
    }

    public Task CreateTagIfNotExists(string tag) => throw new NotImplementedException(nameof(TestDataRepository));

    public void Dispose()
    {
        // _notesTableStub = null;
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return new ValueTask();
    }
}
