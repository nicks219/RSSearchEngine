using System.Text;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Entities;

namespace Rsse.Tests.Common;

/// <summary>
/// Компонент поставщика многократной копии данных из файла.
/// </summary>
public sealed class FileDataMultipleProvider(int initialDataMultiplier = Constants.InitialDataMultiplier) : IDataProvider<NoteEntity>
{
    // Данные дублируются Constants.InitialDataMultiplier раз.
    /// <inheritdoc/>
    public async IAsyncEnumerable<NoteEntity> GetDataAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var notes = new List<NoteEntity>(1000);

        await using var fileStream = File.OpenRead("pg_backup_.txtnotes");
        using var reader = new StreamReader(fileStream);

        StringBuilder stringBuilder = new StringBuilder();

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
                Title = stringBuilder.Clear().Append(items[1])
                    .Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").ToString(),

                Text = stringBuilder.Clear().Append(items[2])
                    .Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").ToString()
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
