#if WINDOWS
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Rsse.Tooling.Contracts;

namespace Rsse.Tooling.DumpAssistant;

/// <summary>
/// Тулинг для конвертации дампа MySql и переименования таблиц.
/// </summary>
/// [
[ExcludeFromCodeCoverage]
[Obsolete("не используется в данной версии сервиса")]
internal class MySqlDumpTooling
{
    private const string DumpName = "backup_9.txt";
    private const string SplitPattern = " ";
    private const string SkipPattern = "VALUES";
    private const bool LogIsEnabled = true;

    private static readonly char DirectorySeparator = Path.DirectorySeparatorChar;
    private static readonly string DumpRelativePath = $"ClientApp{DirectorySeparator}build{DirectorySeparator}";
    private static readonly string DumpAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), DumpRelativePath);

    private readonly string _originalDump = Path.Combine(DumpAbsolutePath, DumpName);

    // текущее значение > эталонное значение
    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly Dictionary<string, string> NamingPattern = new()
    {
        // не идемпотентно, также играет роль порядок применения совокупности правил: song>text, text>note
        // требуется проверка на пересечения key/value и алгоритм их сортировки
        // в начале расположены более длинные слова

        { "GenreText", "TagsToNotes" },
        { "GenreID", "TagId" },
        { "TextID", "NoteId" },
        { "Genre", "Tag" },
        { "Text", "Note" },
        { "Song", "Text" }
    };

    /// <summary>
    /// Переименовать поля дампа.
    /// </summary>
    internal void Rename(Dictionary<string, string> namingPattern)
    {
        var lineCounter = 0;
        var changeCounter = 0;
        var originalLines = File.ReadAllLines(_originalDump);
        var originalLinesCount = originalLines.Length;
        var builtDump = _originalDump + "_";
        var builtLines = new StringBuilder(originalLinesCount);

        // распараллель
        foreach (var line in originalLines)
        {
            if (LogIsEnabled)
            {
                Console.WriteLine($"LINE: {lineCounter}");
            }

            // TODO абстрагировать правила а Func<T,TResult>: split > skip > replace
            // split
            var wordsInLine = line.Split(SplitPattern);
            var wordsInLineCount = wordsInLine.Length;
            var oneLine = new StringBuilder(wordsInLineCount);
            var oneLineWriter = new StringWriter(oneLine) { NewLine = "\n" };

            using (oneLineWriter)
            {
                foreach (var word in wordsInLine)
                {
                    // skip
                    if (word.StartsWith(SkipPattern))
                    {
                        if (LogIsEnabled)
                        {
                            Console.WriteLine($"DROP: {word[..15]}");
                        }

                        WriteWord(oneLineWriter, word);
                        continue;
                    }

                    // replace
                    var currentWord = word;
                    foreach (var rule in namingPattern)
                    {
                        while (currentWord.Contains(rule.Key))
                        {
                            changeCounter++;
                            var temp = currentWord.Replace(rule.Key, rule.Value);
                            currentWord = temp;
                        }
                    }

                    // добавить строку
                    if (currentWord != word && LogIsEnabled)
                    {
                        Console.WriteLine($"REPLACE: {word} > {currentWord}");
                    }

                    WriteWord(oneLineWriter, currentWord);
                }

                // добавить к строкам
                oneLine.Length--;
                TerminateLine(oneLineWriter);
                builtLines.Append(oneLine);
            }

            lineCounter++;
        }

        // записать строки в файл
        File.WriteAllText(builtDump, builtLines.ToString());

        if (LogIsEnabled)
        {
            var dumpSize = new FileInfo(_originalDump).Length;
            var newDumpSize = new FileInfo(builtDump).Length;
            Console.WriteLine($"LINES: {originalLinesCount} > {lineCounter}");
            Console.WriteLine($"SIZE: {dumpSize} > {newDumpSize}");
            Console.WriteLine($"CHANGES: {changeCounter}");
        }

        return;

        void WriteWord(TextWriter writer, string wordToAppend)
        {
            writer.Write(wordToAppend);
            writer.Write(SplitPattern);
        }

        void TerminateLine(TextWriter writer)
        {
            writer.WriteLine();
        }
    }

    /// <summary>Конвертироать дамп.</summary>
    internal static void Convert(IDbConvertor convertor, object ddlFrom, object ddlTo, string pathToDump)
    {
        // пока не решил, как лучше описать схему данных
        convertor.Convert(ddlFrom, ddlTo, pathToDump);
    }
}
#endif
