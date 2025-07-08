using System.Text;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Entities;

namespace Rsse.Tests.Common;

/// <summary>
/// Компонент поставщика многократной копии данных из файла.
/// </summary>
public sealed class BigFileDataMultipleProvider(int initialDataMultiplier = Constants.InitialDataMultiplier) : IDataProvider<NoteEntity>
{
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
                Title = items[1].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
                Text = items[2].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
            };

            notes.Add(noteEntity);
        }

        var counter = 0;

        StringBuilder builder = new StringBuilder();

        for (int index = 0; index < notes.Count; index++)
        {
            NoteEntity entity = notes[index];

            for (int i = index + 1; i < index + (notes.Count / 100); i++)
            {
                builder.Append(notes[i % notes.Count].Text.Substring(0, notes[i % notes.Count].Text.Length / 2));
                builder.Append(" ");
            }

            builder.Append(entity.Text.Substring(0, Math.Min(entity.Text.Length / 2 + 20, entity.Text.Length)));
            builder.Append(" ");

            for (int i = index + 100; i < index + (notes.Count / 100); i++)
            {
                builder.Append(notes[i % notes.Count].Text.Substring(notes[i % notes.Count].Text.Length / 2, notes[i % notes.Count].Text.Length - notes[i % notes.Count].Text.Length / 2));
                builder.Append(" ");
            }

            builder.Append(entity.Text.Substring(entity.Text.Length / 2, entity.Text.Length - (entity.Text.Length / 2)));
            builder.Append(" ");

            NoteEntity noteEntity = new NoteEntity
            {
                NoteId = index,
                Title = entity.Title,
                Text = builder.ToString(),
            };

            builder.Clear();

            yield return noteEntity;

            counter++;
        }

        Console.WriteLine($"[{nameof(FileDataMultipleProvider)}] sent total data: '{counter:N0}' entries | {initialDataMultiplier:N0}x.");
        Console.WriteLine("---");
    }

    public async IAsyncEnumerable<NoteEntity> GetDataAsync2()
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
                Title = items[1].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
                Text = items[2].Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"),
            };

            notes.Add(noteEntity);
        }

        var counter = 0;
        for (var i = 0; i < initialDataMultiplier; i++)
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

        Console.WriteLine($"[{nameof(FileDataMultipleProvider)}] sent total data: '{counter:N0}' entries | {initialDataMultiplier:N0}x.");
        Console.WriteLine("---");
    }
}
