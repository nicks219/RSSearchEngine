using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Domain.Elector;

internal static class NoteElector
{
    private static readonly Random Random = new();
    private static uint _id;

    /// <summary>
    /// Выбрать идентификатор заметки из списка, случайно или раунд-робином
    /// </summary>
    /// <param name="electableNoteIds">идентификаторы заметок, участвующих в выборе</param>
    /// <param name="randomElectionEnabled">алгоритм выбора</param>
    /// <returns>идентификатор выбранной заметки</returns>
    internal static int ElectNextNote(List<int> electableNoteIds, bool randomElectionEnabled = true)
    {
        var electableNoteCount = electableNoteIds.Count;

        if (electableNoteCount == 0)
        {
            return 0;
        }

        Interlocked.Increment(ref _id);

        // round-robin либо random, отсчёт от нуля:
        var coin = randomElectionEnabled ? GetRandomInRange(electableNoteCount) : (int)(_id % electableNoteCount);

        // в данный момент дополнительная рандомизация Shuffle не задействована:
        var nextId = electableNoteIds
            .OrderBy(s => s)
            .ElementAt(coin);

        return nextId;
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
