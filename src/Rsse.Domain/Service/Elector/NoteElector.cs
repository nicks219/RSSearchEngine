using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rsse.Domain.Service.Configuration;

namespace Rsse.Domain.Service.Elector;

/// <summary>
/// Функционал для выбора заметок.
/// </summary>
internal static class NoteElector
{
    private static readonly Random Random = new();
    private static uint _id;
    private static int _prevCoin;

    /// <summary>
    /// Выбрать идентификатор заметки из списка, случайно или раунд-робином.
    /// </summary>
    /// <param name="electableNoteIds">Идентификаторы заметок, участвующих в выборе.</param>
    /// <param name="electionType">Алгоритм выбора следующей заметки.</param>
    /// <returns>Идентификатор выбранной заметки.</returns>
    internal static int ElectNextNote(List<int> electableNoteIds, ElectionType electionType = ElectionType.Rng)
    {
        if (electionType == ElectionType.SqlRandom)
        {
            throw new NotImplementedException($"{nameof(ElectionType.SqlRandom)} not used for {nameof(ElectNextNote)} method");
        }

        var electableNoteCount = electableNoteIds.Count;

        if (electableNoteCount == 0)
        {
            return 0;
        }

        Interlocked.Increment(ref _id);

        // Round-robin либо random, отсчёт от нуля.
        var coin = electionType == ElectionType.RoundRobin
            ? (int)(_id % electableNoteCount)
            : GetRandomInRange(electableNoteCount);

        if (electionType == ElectionType.Unique)
        {
            if (_prevCoin == coin)
            {
                // Смещаем "монетку" если прошлый раз выпало такое же значение, сомнительная реализация.
                // todo: можно обеспечить уникальный результат для уникального набора идентификаторов в рамках какой-то части вызовов.
                coin = ++coin % electableNoteCount;
            }

            _prevCoin = coin;
        }

        // Дополнительная рандомизация Shuffle не задействована.
        var nextId = electableNoteIds
            .OrderBy(s => s)
            .ElementAt(coin);

        return nextId;
    }

    /// <summary>
    /// Потокобезопасная генерация случайного числа в заданном непрерывном диапазоне.
    /// </summary>
    /// <param name="exclusiveUpperBound">Количество заметок, доступных для выбора.</param>
    /// <returns>Идентификатор случайной заметки.</returns>
    private static int GetRandomInRange(int exclusiveUpperBound)
    {
        lock (Random)
        {
            var coin = Random.Next(0, exclusiveUpperBound);

            return coin;
        }
    }
}
