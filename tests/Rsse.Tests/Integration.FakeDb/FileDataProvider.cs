using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Entities;

namespace Rsse.Tests.Integration.FakeDb;

/// <summary>
/// Компонент поставщика данных из файла.
/// </summary>
public sealed class FileDataProvider : IDataProvider<NoteEntity>
{
    private readonly int initialDataMultiplier = 1;

    // Данные дублируются Constants.InitialDataMultiplier раз.
    /// <inheritdoc/>
    public async IAsyncEnumerable<NoteEntity> GetDataAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var notes = new List<NoteEntity>(1000);

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
                NoteId = int.Parse(items[0]),
                Title = items[1].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
                Text = items[2].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
            };

            notes.Add(noteEntity);
        }

        //var counter = 0;
        for (var i = 0; i < initialDataMultiplier; i++)
        {
            foreach (var noteEntity in notes/*.Select(entity =>
                         new NoteEntity
                         {
                             NoteId = counter,
                             Title = entity.Title,
                             Text = entity.Text,
                         })*/)
            {
                //counter++;
                yield return noteEntity;
            }
        }

        //Console.WriteLine($"[{nameof(FileDataProvider)}] sent total data: '{counter:N0}' entries | {initialDataMultiplier:N0}x.");
        //Console.WriteLine("---");
    }
}
