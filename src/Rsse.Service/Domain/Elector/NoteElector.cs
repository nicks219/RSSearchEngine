using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SearchEngine.Domain.Elector;

/// <summary>
/// Функционал для выбора заметок.
/// </summary>
internal static class NoteElector
{
    private static readonly Random Random = new();
    private static uint _id;

    /// <summary>
    /// Выбрать идентификатор заметки из списка, случайно или раунд-робином.
    /// </summary>
    /// <param name="electableNoteIds">Идентификаторы заметок, участвующих в выборе.</param>
    /// <param name="randomElectionEnabled">Алгоритм выбора, <b>true</b> - случайный выбор.</param>
    /// <returns>Идентификатор выбранной заметки.</returns>
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
    /// Потокобезопасная генерация случайного числа в заданном непрерывном диапазоне.
    /// </summary>
    /// <param name="howMany">Количество заметок, доступных для выбора.</param>
    /// <returns>Идентификатор случайной заметки.</returns>
    private static int GetRandomInRange(int howMany)
    {
        lock (Random)
        {
            var coin = Random.Next(0, howMany);

            return coin;
        }
    }
}
