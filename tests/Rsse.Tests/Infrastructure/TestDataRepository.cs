using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RandomSongSearchEngine.Data;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Tests.Infrastructure;

public class TestDataRepository : IDataRepository
{
    private int _id;
    
    public async Task CreateTagIfNotExists(string tag){}
    
    private Dictionary<int, Tuple<string, string>> _dictionary = new();

    private const string FirstSong = "Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\n";

    private const string FirstSongTitle = "Розенбаум - Вечерняя застольная";
    
    public const string SecondSong =  "Облака, белогривыи лошадки, облака, что ж вы мчитесь?\r\n";

    public const string SecondSongTitle = "Шаинский - Облака";

    public IQueryable<TextEntity> ReadAllNotes()
    {
        var songs = new List<TextEntity> {new() {Song = FirstSong, Title = FirstSongTitle, TextId = 1}}.AsQueryable();

        return songs;
    }

    public string ReadTitleByNoteId(int id)
    {
        throw new NotImplementedException();
    }

    public int FindNoteIdByTitle(string noteTitle)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateNote(NoteDto? dt)
    {
        if (dt?.Title == null || dt.Text == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }
        
        _dictionary.Add(_id, new Tuple<string, string>(dt.Title, dt.Text));
        _id++;
        return new Task<int>(() => _id - 1);
    }

    public Task<int> DeleteNote(int noteId)
    {
        var res = _dictionary.Remove(noteId);
        return new Task<int>(() => Convert.ToInt32(res));
    }

    public void Dispose()
    {
        _dictionary = null!;
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }

    //Login Ok
    public Task<UserEntity?> GetUser(LoginDto dt)
    {
        var user = new UserEntity();
        return new Task<UserEntity?>(() => user);
    }

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int lastPage, int pageSize)
    {
        var q = new List<Tuple<string, int>>
        {
            new(_dictionary[1].Item1, 1)
        };
        return q.AsQueryable();
    }

    public Task<List<string>> ReadGeneralTagList()
    {
        return new Task<List<string>>(() => new List<string>() {"Rock", "Pop", "Jazz"});
    }

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        var q = new List<Tuple<string, string>>
        {
            _dictionary[noteId]
        };
        return q.AsQueryable();
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var l = new List<int>();
        var r = _dictionary[noteId];
        if (r == null)
        {
            l.Add(1);
            l.Add(2);
        }

        return l.AsQueryable();
    }

    public Task<int> ReadNotesCount()
    {
        return new Task<int>(() => _dictionary.Count);
    }

    public IQueryable<int> ReadAllNotesTaggedBy(IEnumerable<int> checkedTags)
    {
        var l = new List<int> {1, 2, 3};
        return l.AsQueryable();
    }

    public Task UpdateNote(IEnumerable<int> initialTags, NoteDto? dt)
    {
        if (dt?.Title == null || dt.Text == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }
        
        _dictionary[dt.Id] = new Tuple<string, string>(dt.Title, dt.Text);
        return new Task(() => Console.Write(""));
    }
}

public class FakeServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceScope _serviceScope;

    public IServiceScope CreateScope()
    {
        return _serviceScope;
    }

    public FakeServiceScopeFactory(IServiceScope serviceScope)
    {
        _serviceScope = serviceScope;
    }
}
