﻿using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Infrastructure.Engine;

public static class Randomizer
{
    private static readonly Random Random = new();
    private static uint _id;

    /// <summary> Возвращает Id случайно выбранной песни из заданных категорий </summary>
    public static async Task<int> GetBalancedIdAsync(this IDataRepository repo, List<int> songGenresRequest, bool isRandom = false)
    {
        var checkedGenres = songGenresRequest.ToArray();
        
        var allSongsInGenres = repo.SelectAllSongsInGenres(checkedGenres);
        var howManySongs = await allSongsInGenres.CountAsync();
        
        if (howManySongs == 0)
        {
            return 0;
        }

        // Random
        /*var coin = GetRandomInRange(howManySongs);
        var randomResult = await repo.SelectAllSongsInGenres(checkedGenres)
            //[WARNING] [Microsoft.EntityFrameworkCore.Query]  The query uses a row limiting operator ('Skip'/'Take')
            // without an 'OrderBy' operator.
            .OrderBy(s => s)
            .Skip(coin)
            .Take(1)
            .FirstAsync();*/

        // RoundRobin
        Interlocked.Increment(ref _id);
        
        // Or Random
        // TODO isRandom должен настраиваться через конфигурацию.
        var coin = isRandom ? GetRandomInRange(howManySongs) : (int)(_id % howManySongs);

        var roundRobinResult = await allSongsInGenres
            // [WARNING] [Microsoft.EntityFrameworkCore.Query]  The query uses a row limiting operator ('Skip'/'Take')
            // without an 'OrderBy' operator.
            .OrderBy(s => s)
            .Skip(coin)
            .Take(1)
            .FirstAsync();
        
        return roundRobinResult;
    }

    /// <summary>
    /// Потокобезопасная генерация случайного числа в заданном диапазоне
    /// </summary>
    /// <param name="howMany">Количество песен, доступных для выборки</param>
    /// <returns></returns>
    private static int GetRandomInRange(int howMany)
    {
        lock (Random)
        {
            var coin = Random.Next(0, howMany);
            
            return coin;
        }
    }
}