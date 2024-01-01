using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Tests.Units.Mocks.DatabaseRepo;

internal class TestCatalogRepository : IDataRepository
{
    internal const string FirstNoteText = "Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\n";
    internal const string FirstNoteTitle = "Розенбаум - Вечерняя застольная";
    internal const string SecondNoteText = "Облака, белогривыи лошадки, облака, что ж вы мчитесь?\r\n";
    internal const string SecondNoteTitle = "Шаинский - Облака";

    internal static readonly List<string> TagList = new() { "Rock", "Pop", "Jazz" };

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

    public IQueryable<NoteEntity> ReadAllNotes()
    {
        var notes = _notesTableStub?
            .Select(keyValue => new NoteEntity
            {
                Text = keyValue.Value.Item2,
                Title = keyValue.Value.Item1,
                NoteId = keyValue.Key
            })
            .ToList();

        return new TestQueryable<NoteEntity>(notes!);
    }

    public string ReadNoteTitle(int noteId)
    {
        if (_notesTableStub == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notesTableStub.Where(keyValue => keyValue.Key == noteId))
        {
            return keyValue.Value.Item1;
        }

        throw new NotImplementedException($"Note with id `{noteId}` not found");
    }

    public int ReadNoteId(string noteTitle)
    {
        if (_notesTableStub == null) throw new NullReferenceException("Data is null");

        foreach (var keyValue in _notesTableStub.Where(keyValue => keyValue.Value.Item1 == noteTitle))
        {
            return keyValue.Key;
        }

        throw new NotImplementedException($"Note `{noteTitle}` not found");
    }

    public Task<int> CreateNote(NoteDto? note)
    {
        if (note?.TitleRequest == null || note.TextRequest == null || _notesTableStub == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notesTableStub.Add(_id, new Tuple<string, string>(note.TitleRequest, note.TextRequest));
        _id++;
        return Task.FromResult(_id - 1);
    }

    public Task<int> DeleteNote(int noteId)
    {
        var res = _notesTableStub?.Remove(noteId);
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

    public IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags)
    {
        return new TestQueryable<int>(checkedTags);
    }

    public Task UpdateNote(IEnumerable<int> initialTags, NoteDto? note)
    {
        if (note?.TitleRequest == null || note.TextRequest == null || _notesTableStub == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }

        _notesTableStub[note.NoteId] = new Tuple<string, string>(note.TitleRequest, note.TextRequest);

        return Task.CompletedTask;
    }

    public Task CreateTagIfNotExists(string tag) => throw new NotImplementedException(nameof(TestCatalogRepository));

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return new ValueTask();
    }
}
