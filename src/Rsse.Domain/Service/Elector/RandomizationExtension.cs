using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SearchEngine.Service.Elector;

/// <summary>
/// Расширение для дополнительной рандомизации.
/// </summary>
public static class RandomizationExtension
{
    /// <summary>
    /// Перемешать список, качество зависит от RNG.
    /// </summary>
    public static IList<T> Shuffle<T>(this IList<T> list, RandomNumberGenerator rng)
    {
        var current = list.Count;
        while (current > 1)
        {
            // var k = rng.Next(n--);
            var next = GetNextInt32(rng) % current--;
            (list[current], list[next]) = (list[next], list[current]);
        }

        return list;
    }

    /// <summary>
    /// Получить случайное число с использованием криптостойкого RNG для генерации.
    /// </summary>
    private static int GetNextInt32(RandomNumberGenerator rnd)
    {
        var randomInt = new byte[4];
        rnd.GetBytes(randomInt);
        return BitConverter.ToInt32([randomInt[0]]);
    }
}
