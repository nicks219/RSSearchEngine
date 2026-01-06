using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Entities;
using Rsse.Tests.Common;
using RsseEngine.Dto.Common;
using RsseEngine.SearchType;
using RsseEngine.Service;

namespace RsseEngine.Benchmarks.Validation;

/// <summary>
/// Оценка результатов поиска.
/// </summary>
public class SearchResultValidation
{
    private readonly List<ExtendedSearchType> _extendedParameters =
        ((ExtendedSearchType[])Enum.GetValuesAsUnderlyingType<ExtendedSearchType>())
        .Where(searchType => searchType != ExtendedSearchType.Legacy)
        .ToList();

    private readonly List<ReducedSearchType> _reducedParameters =
        ((ReducedSearchType[])Enum.GetValuesAsUnderlyingType<ReducedSearchType>())
        .Where(searchType => searchType != ReducedSearchType.Legacy)
        .ToList();

    private readonly List<string> _searchQueries =
    [
        "пляшем на столе за детей",
        "удача с ними за столом",
        "чорт з ным зо сталом",
        "чёрт с ними за столом",
        "с ними за столом чёрт",
        "преключиться вдруг верный друг",
        "приключится вдруг верный друг",
        "приключится вдруг вот верный друг выручить",
        "пляшем на",
        "ты шла по палубе в молчаний",
        "оно шла по палубе в молчаний",
        "123 456 иии",
        "aa bb cc dd .,/#",
        " |",
        "я ты он она",
        "a b c d .,/#",
        " ",
        "",
        "b b b b b b",
        "b b b b b",
        "b b b b",
        "b"
    ];

    private readonly List<NoteEntity> _additionalNotes =
    [
        new() { NoteId = 10000, Title = "t", Text = "b b b b b" }
    ];

    public async Task TestSearchQuery()
    {
        Console.WriteLine();
        Console.WriteLine(nameof(TestSearchQuery));

        var dataProvider = new FileDataOnceProvider();
        dataProvider.AddNotes(_additionalNotes);

        var legacyTokenizer = await InitializeTokenizer(dataProvider,
            ExtendedSearchType.Legacy, ReducedSearchType.Legacy);

        foreach (ExtendedSearchType extendedSearchType in _extendedParameters)
        {
            foreach (string searchQuery in _searchQueries)
            {
                var tokenizer = await InitializeTokenizer(dataProvider, extendedSearchType, ReducedSearchType.Legacy);

                var legacyExtended = FindExtended(legacyTokenizer, searchQuery);
                var extendedResult = FindExtended(tokenizer, searchQuery);

                CompareResult(extendedSearchType, ReducedSearchType.Legacy, legacyExtended, extendedResult,
                    searchQuery);
            }
        }

        foreach (ReducedSearchType reducedSearchType in _reducedParameters)
        {
            foreach (string searchQuery in _searchQueries)
            {
                var tokenizer = await InitializeTokenizer(dataProvider, ExtendedSearchType.Legacy, reducedSearchType);

                var legacyReduced = FindReduced(legacyTokenizer, searchQuery);
                var reducedResult = FindReduced(tokenizer, searchQuery);

                CompareResult(ExtendedSearchType.Legacy, reducedSearchType, legacyReduced, reducedResult, searchQuery);
            }
        }
    }

    public async Task TestDuplicates()
    {
        var dataProvider = new FileDataOnceProvider();
        dataProvider.AddNotes([new() { NoteId = 10000, Title = "t", Text = "b b b b b" }]);

        var legacyTokenizer = await InitializeTokenizer(dataProvider, ExtendedSearchType.Legacy, ReducedSearchType.Legacy);

        List<NoteEntity> noteEntities = new();

        await foreach (var noteEntity in dataProvider.GetDataAsync())
        {
            noteEntities.Add(noteEntity);
        }

        Console.WriteLine();
        Console.WriteLine($"{nameof(TestDuplicates)}.{nameof(FindExtended)}");

        foreach (ExtendedSearchType extendedSearchType in _extendedParameters)
        {
            var tokenizer = await InitializeTokenizer(dataProvider, extendedSearchType, ReducedSearchType.Legacy);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var noteEntity in noteEntities)
            {
                var legacyExtended = FindExtended(legacyTokenizer, noteEntity.Text);
                var extendedResult = FindExtended(tokenizer, noteEntity.Text);

                CompareResult(extendedSearchType, ReducedSearchType.Legacy, legacyExtended, extendedResult);
            }

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms {extendedSearchType}");
        }

        Console.WriteLine();
        Console.WriteLine($"{nameof(TestDuplicates)}.{nameof(FindReduced)}");

        foreach (var reducedSearchType in _reducedParameters)
        {
            var tokenizer = await InitializeTokenizer(dataProvider, ExtendedSearchType.Legacy, reducedSearchType);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (NoteEntity noteEntity in noteEntities)
            {
                var legacyReduced = FindReduced(legacyTokenizer, noteEntity.Text);
                var reducedResult = FindReduced(tokenizer, noteEntity.Text);

                CompareResult(ExtendedSearchType.Legacy, reducedSearchType, legacyReduced, reducedResult);
            }

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms {reducedSearchType}");
        }
    }

    private static void CompareResult(
        ExtendedSearchType extendedSearchType,
        ReducedSearchType reducedSearchType,
        List<KeyValuePair<DocumentId, double>> legacy,
        List<KeyValuePair<DocumentId, double>> result,
        string searchQuery = "")
    {
        if (legacy.Count != result.Count)
        {
            Console.WriteLine($"extended[{extendedSearchType}] reduced[{reducedSearchType}]"
                              + $" Count legacy[{legacy.Count}] result[{result.Count}]"
                              + $" {searchQuery}");
        }

        for (var index = 0; index < legacy.Count; index++)
        {
            var legacyKeyValuePair = legacy[index];
            var resultKeyValuePair = result[index];

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (legacyKeyValuePair.Key != resultKeyValuePair.Key ||
                legacyKeyValuePair.Value != resultKeyValuePair.Value)
            {
                Console.WriteLine($"extended[{extendedSearchType}] reduced[{reducedSearchType}]"
                                  + $" Key legacy[{legacyKeyValuePair.Key}] result[{resultKeyValuePair.Key}]"
                                  + $" Value legacy[{legacyKeyValuePair.Value}] result[{resultKeyValuePair.Value}]"
                                  + $" {searchQuery}");
            }
        }
    }

    private static async Task<TokenizerServiceCore> InitializeTokenizer(FileDataOnceProvider dataProvider,
        ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        var tokenizer = new TokenizerServiceCore(MetricsCalculatorType.PooledMetricsCalculator,
            false, extendedSearchType, reducedSearchType);

        var result = await tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        return tokenizer;
    }

    private static List<KeyValuePair<DocumentId, double>> FindExtended(TokenizerServiceCore tokenizer, string query)
    {
        var metricsCalculator = tokenizer.CreateMetricsCalculator();

        try
        {
            tokenizer.ComputeComplianceIndexExtended(query, metricsCalculator, CancellationToken.None);

            var result = metricsCalculator.ComplianceMetrics
                .Select(kvp => new KeyValuePair<DocumentId, double>(new DocumentId(kvp.Key), kvp.Value))
                .OrderBy(t => t.Key)
                .ToList();

            return result;
        }
        finally
        {
            tokenizer.ReleaseMetricsCalculator(metricsCalculator);
        }
    }

    private static List<KeyValuePair<DocumentId, double>> FindReduced(TokenizerServiceCore tokenizer, string query)
    {
        var metricsCalculator = tokenizer.CreateMetricsCalculator();

        try
        {
            tokenizer.ComputeComplianceIndexReduced(query, metricsCalculator, CancellationToken.None);

            var result = metricsCalculator.ComplianceMetrics
                .Select(kvp => new KeyValuePair<DocumentId, double>(new DocumentId(kvp.Key), kvp.Value))
                .OrderBy(t => t.Key)
                .ToList();

            return result;
        }
        finally
        {
            tokenizer.ReleaseMetricsCalculator(metricsCalculator);
        }
    }
}
