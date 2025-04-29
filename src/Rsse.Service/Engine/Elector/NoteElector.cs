using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Engine.Elector;

internal static class NoteElector
{
    private static readonly Random Random = new();
    private static uint _id;

    /// <summary>
    /// Выбрать заметку по заданным тегам, случайно или раунд-робином
    /// </summary>
    /// <param name="electableNoteIds">идентификаторы заметок, участвующих в выборе</param>
    /// <param name="randomElectionEnabled">алгоритм выбора</param>
    /// <returns>идентификатор выбранной заметки</returns>
    internal static async Task<int> ElectNextNoteAsync(IQueryable<int> electableNoteIds, bool randomElectionEnabled = true)
    {
        var electableNoteCount = await electableNoteIds.CountAsync();

        if (electableNoteCount == 0)
        {
            return 0;
        }

        Interlocked.Increment(ref _id);

        // round-robin либо random:
        var coin = randomElectionEnabled ? GetRandomInRange(electableNoteCount) : (int)(_id % electableNoteCount);

        // в данный момент дополнительная рандомизация не задействована:
        var result = randomElectionEnabled switch
        {
            // случайный выбор:
            true => await electableNoteIds
                // дополнительное перемешивание:
                // .ToList().Shuffle(random)
                // .OrderBy(s => GetNextInt32(random))
                .OrderBy(s => s)
                .Skip(coin)
                .Take(1)
                .FirstAsync(),

            // round-robin:
            _ => await electableNoteIds
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
    /// <param name="howMany">Количество песен, доступных для выбора</param>
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
