using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Infrastructure.Engine;

public static class NoteElector
{
    private static readonly Random Random = new();
    private static uint _id;

    /// <summary> Возвращает Id выбранной случайно или раунд-робином песни из заданных категорий </summary>
    public static async Task<int> ElectNoteId(this IDataRepository repo, List<int> checkedTagsList, bool randomElection = true)
    {
        var checkedTags = checkedTagsList.ToArray();

        var allElectableNotes = repo.ReadAllNotesTaggedBy(checkedTags);
        var howManyNotes = await allElectableNotes.CountAsync();

        if (howManyNotes == 0)
        {
            return 0;
        }

        // Random
        /*var coin = GetRandomInRange(howManySongs);
        var randomResult = await repo.ReadAllNotesTaggedBy(checkedGenres)
            //[WARNING] [Microsoft.EntityFrameworkCore.Query]  The query uses a row limiting operator ('Skip'/'Take')
            // without an 'OrderBy' operator.
            .OrderBy(s => s)
            .Skip(coin)
            .Take(1)
            .FirstAsync();*/

        Interlocked.Increment(ref _id);

        // RoundRobin Or Random
        var coin = randomElection ? GetRandomInRange(howManyNotes) : (int) (_id % howManyNotes);
        
        //var random = RandomNumberGenerator.Create();
        var result = randomElection switch
        {
            // [original random]
            true => await allElectableNotes
                // [WARNING] [Microsoft.EntityFrameworkCore.Query]  The query uses a row limiting operator ('Skip'/'Take')
                // without an 'OrderBy' operator.
                //.ToList().Shuffle(random) // - дополнительное перемешивание
                //.OrderBy(s => GetNextInt32(random)) // - дополнительное перемешивание
                .OrderBy(s => s)
                .Skip(coin)
                .Take(1)
                .FirstAsync(),

            // [round robin]
            _ => await allElectableNotes
                // [WARNING] [Microsoft.EntityFrameworkCore.Query]  The query uses a row limiting operator ('Skip'/'Take')
                // without an 'OrderBy' operator.
                .OrderBy(s => s)
                .Skip(coin)
                .Take(1)
                .FirstAsync()
        };

        return result;
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
    
    /// <summary>
    /// Использование криптостойкого RNG для генерации.
    /// </summary>
    private static int GetNextInt32(RandomNumberGenerator rnd)
    {
        var randomInt = new byte[4];
        rnd.GetBytes(randomInt);
        return Convert.ToInt32(randomInt[0]);
    }
    
    /// <summary>
    /// Перемешивание списка, качество зависит от RNG.
    /// </summary>
    private static IList<T> Shuffle<T> (this IList<T> list, RandomNumberGenerator rng)
    {
        var n = list.Count;
        while (n > 1) 
        {
            // var k = rng.Next(n--);
            var k = GetNextInt32(rng) % n--;
            (list[n], list[k]) = (list[k], list[n]);
        }

        return list;
    }
}