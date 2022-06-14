using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RandomSongSearchEngine.Data;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Tests.Infrastructure;

public class TestDataRepository : IDataRepository
{
    private int _id;
    
    private Dictionary<int, Tuple<string, string>> _dictionary = new();

    private const string FirstSong = "1,'Розенбаум - Вечерняя застольная'," +
                                "'Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\n'";
    
    public const string SecondSong = "2,'Шаинский - Облака'," +
                                "'Облака, белогривыи лошадки, облака, что ж вы мчитесь?\r\n'";

    public IQueryable<string> ReadAllSongs()
    {
        var songs = new List<string> {FirstSong}.AsQueryable();

        return songs;
    }

    public string ReadSongTitleById(int id)
    {
        throw new NotImplementedException();
    }

    public int FindIdByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateSongAsync(SongDto? dt)
    {
        if (dt?.Title == null || dt.Text == null)
        {
            throw new NullReferenceException("[TestRepository: data error]");
        }
        
        _dictionary.Add(_id, new Tuple<string, string>(dt.Title, dt.Text));
        _id++;
        return new Task<int>(() => _id - 1);
    }

    public Task<int> DeleteSongAsync(int songId)
    {
        var res = _dictionary.Remove(songId);
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

    public Task<List<string>> ReadGenreListAsync()
    {
        return new Task<List<string>>(() => new List<string>() {"Rock", "Pop", "Jazz"});
    }

    public IQueryable<Tuple<string, string>> ReadSong(int textId)
    {
        var q = new List<Tuple<string, string>>
        {
            _dictionary[textId]
        };
        return q.AsQueryable();
    }

    public IQueryable<int> ReadSongGenres(int textId)
    {
        var l = new List<int>();
        var r = _dictionary[textId];
        if (r == null)
        {
            l.Add(1);
            l.Add(2);
        }

        return l.AsQueryable();
    }

    public Task<int> ReadTextsCountAsync()
    {
        return new Task<int>(() => _dictionary.Count);
    }

    public IQueryable<int> SelectAllSongsInGenres(IEnumerable<int> checkedGenres)
    {
        var l = new List<int> {1, 2, 3};
        return l.AsQueryable();
    }

    public Task UpdateSongAsync(IEnumerable<int> originalCheckboxes, SongDto? dt)
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
