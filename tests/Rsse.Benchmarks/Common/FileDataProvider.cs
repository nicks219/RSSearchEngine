using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Entities;

namespace SearchEngine.Benchmarks.Common;

/// <summary>
/// Компонент поставщика данных из файла.
/// </summary>
public sealed class FileDataProvider : IDataProvider<NoteEntity>
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<NoteEntity> GetDataAsync()
    {
        var counter = 0;
        Console.OutputEncoding = Encoding.UTF8;

        await using var fileStream = File.OpenRead("pg_backup_.txtnotes");
        using var reader = new StreamReader(fileStream);
        for (var text = await reader.ReadLineAsync(); text != null; text = await reader.ReadLineAsync())
        {
            var items = text.Split('\t');

            if (items.Length != 3)
            {
                Console.WriteLine("FILE DATA PROVIDER: BROKEN CSV STRING");
                throw new Exception("Unexpected number of items");
            }

            var noteEntity = new NoteEntity
            {
                NoteId = counter,
                Title = items[1].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
                Text = items[2].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
            };
            counter++;
            yield return noteEntity;
        }

        Console.WriteLine($"[{nameof(FileDataProvider)}] total data: '{counter}'");
    }
}
