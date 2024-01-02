using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Engine.Elector;

internal static class NoteElector
{
    private static readonly Random Random = new();
    private static uint _id;

    /// <summary>
    /// Выбрать заметку по заданным тегам, случайно или раунд-робином
    /// </summary>
    /// <param name="repo">репозиторий с данными</param>
    /// <param name="checkedTagsList">отмеченные теги</param>
    /// <param name="runRandomElection">алгоритм выбора</param>
    /// <returns>идентификатор выбранной заметки</returns>
    internal static async Task<int> ElectNextNote(this IDataRepository repo, List<int> checkedTagsList, bool runRandomElection = true)
    {
        var checkedTags = checkedTagsList.ToArray();

        var allElectableNotes = repo.ReadTaggedNotes(checkedTags);
        var howManyNotes = await allElectableNotes.CountAsync();

        if (howManyNotes == 0)
        {
            return 0;
        }

        Interlocked.Increment(ref _id);

        // round-robin либо random:
        var coin = runRandomElection ? GetRandomInRange(howManyNotes) : (int)(_id % howManyNotes);

        // в данный момент дополнительная рандомизация не задействована:
        var result = runRandomElection switch
        {
            // случайный выбор:
            true => await allElectableNotes
                // дополнительное перемешивание:
                // .ToList().Shuffle(random)
                // .OrderBy(s => GetNextInt32(random))
                .OrderBy(s => s)
                .Skip(coin)
                .Take(1)
                .FirstAsync(),

            // round-robin:
            _ => await allElectableNotes
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
