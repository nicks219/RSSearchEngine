using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Infrastructure.Engine;

public static class Randomizer
{
    private static readonly Random Random = new();

    /// <summary> Возвращает Id случайно выбранной песни из заданных категорий </summary>
    public static async Task<int> ReadRandomIdAsync(this IDataRepository repo, List<int> songGenresRequest)
    {
        var checkedGenres = songGenresRequest.ToArray();
        
        var howManySongs = await repo.SelectAllSongsInGenres(checkedGenres).CountAsync();
        
        if (howManySongs == 0)
        {
            return 0;
        }

        var coin = GetRandom(howManySongs);
        
        var result = await repo.SelectAllSongsInGenres(checkedGenres)
            //[WARNING] [Microsoft.EntityFrameworkCore.Query]  The query uses a row limiting operator ('Skip'/'Take')
            // without an 'OrderBy' operator.
            .OrderBy(s => s)
            .Skip(coin)
            .Take(1)
            .FirstAsync();
        
        return result;
    }

    /// <summary>
    /// Потокобезопасная генерация случайного числа в заданном диапазоне
    /// </summary>
    /// <param name="howMany">Количество песен, доступных для выборки</param>
    /// <returns></returns>
    private static int GetRandom(int howMany)
    {
        lock (Random)
        {
            var coin = Random.Next(0, howMany);
            
            return coin;
        }
    }
}