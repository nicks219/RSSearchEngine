using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Entities;
using static SearchEngine.Benchmarks.Constants;

namespace SearchEngine.Benchmarks.Common;

/// <summary>
/// Компонент поставщика данных из файла.
/// </summary>
public sealed class FileDataProvider : IDataProvider<NoteEntity>
{
    // Данные дублируются Constants.InitialDataMultiplier раз.
    /// <inheritdoc/>
    public async IAsyncEnumerable<NoteEntity> GetDataAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var notes = new List<NoteEntity>(InitialDataMultiplier);

        await using var fileStream = File.OpenRead("pg_backup_.txtnotes");
        using var reader = new StreamReader(fileStream);
        while (await reader.ReadLineAsync() is { } text)
        {
            var items = text.Split('\t');

            if (items.Length != 3)
            {
                Console.WriteLine("FILE DATA PROVIDER: BROKEN CSV STRING");
                throw new Exception("Unexpected number of items");
            }

            var noteEntity = new NoteEntity
            {
                Title = items[1].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
                Text = items[2].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
            };

            notes.Add(noteEntity);
        }

        var counter = 0;
        for (var i = 0; i < InitialDataMultiplier; i++)
        {
            foreach (var noteEntity in notes.Select(entity =>
                         new NoteEntity
                         {
                             NoteId = counter,
                             Title = entity.Title,
                             Text = entity.Text,
                         }))
            {
                counter++;
                yield return noteEntity;
            }
        }

        Console.WriteLine($"[{nameof(FileDataProvider)}] sent total data: '{counter:N0}' entries.");
        Console.WriteLine("---");
    }
}
