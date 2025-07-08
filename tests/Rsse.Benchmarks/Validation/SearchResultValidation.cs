using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Entities;
using Rsse.Tests.Common;
using RsseEngine.Dto;
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

    public async Task TestSearchQuery()
    {
        Console.WriteLine();
        Console.WriteLine(nameof(TestSearchQuery));

        var dataProvider = new FileDataMultipleProvider(1);

        var legacyTokenizer = await InitializeTokenizer(dataProvider, ExtendedSearchType.Legacy, ReducedSearchType.Legacy);

        foreach (ExtendedSearchType extendedSearchType in _extendedParameters)
        {
            var tokenizer = await InitializeTokenizer(dataProvider, extendedSearchType, ReducedSearchType.Legacy);

            var legacyExtended = FindExtended(legacyTokenizer, Constants.SearchQuery);
            var extendedResult = FindExtended(tokenizer, Constants.SearchQuery);

            CompareResult(extendedSearchType, ReducedSearchType.Legacy, legacyExtended, extendedResult);
        }

        foreach (ReducedSearchType reducedSearchType in _reducedParameters)
        {
            var tokenizer = await InitializeTokenizer(dataProvider, ExtendedSearchType.Legacy, reducedSearchType);

            var legacyReduced = FindReduced(legacyTokenizer, Constants.SearchQuery);
            var reducedResult = FindReduced(tokenizer, Constants.SearchQuery);

            CompareResult(ExtendedSearchType.Legacy, reducedSearchType, legacyReduced, reducedResult);
        }
    }

    public async Task TestDuplicates()
    {
        var dataProvider = new FileDataMultipleProvider(1);

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
        List<KeyValuePair<DocumentId, double>> result)
    {
        if (legacy.Count != result.Count)
        {
            Console.WriteLine($"extended[{extendedSearchType}] reduced[{reducedSearchType}]"
                              + $" Count legacy[{legacy.Count}] result[{result.Count}]");
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
                                  + $" Value legacy[{legacyKeyValuePair.Value}] result[{resultKeyValuePair.Value}]");
            }
        }
    }

    private static async Task<TokenizerServiceCore> InitializeTokenizer(FileDataMultipleProvider dataProvider,
        ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        var tokenizer = new TokenizerServiceCore(false, extendedSearchType, reducedSearchType);

        var result = await tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        return tokenizer;
    }

    private static List<KeyValuePair<DocumentId, double>> FindExtended(TokenizerServiceCore tokenizer, string query)
    {
        var result = tokenizer.ComputeComplianceIndexExtended(query, CancellationToken.None)
            .OrderBy(t => t.Key)
            .ToList();

        return result;
    }

    private static List<KeyValuePair<DocumentId, double>> FindReduced(TokenizerServiceCore tokenizer, string query)
    {
        var result = tokenizer.ComputeComplianceIndexReduced(query, CancellationToken.None)
            .OrderBy(t => t.Key)
            .ToList();

        return result;
    }
}
