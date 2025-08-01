using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Tests.Common;
using RsseEngine.Benchmarks.Common;
using RsseEngine.SearchType;
using RsseEngine.Service;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск тестового запроса во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class StQueryBenchmarkExtended : IBenchmarkRunner
{
    private const int QueriesCount = 1000;

    private TokenizerServiceCore _tokenizer = null!;

    public static List<BenchmarkParameter<ExtendedSearchType>> Parameters =>
    [
        new(ExtendedSearchType.Legacy),
        new(ExtendedSearchType.GinSimple),
        new(ExtendedSearchType.GinOptimized),
        new(ExtendedSearchType.GinOptimized, true),
        new(ExtendedSearchType.GinFilter),
        new(ExtendedSearchType.GinFilter, true),
        new(ExtendedSearchType.GinFast),
        new(ExtendedSearchType.GinFast, true),
        new(ExtendedSearchType.GinFastFilter),
        new(ExtendedSearchType.GinFastFilter, true),
        new(ExtendedSearchType.GinMerge),
        new(ExtendedSearchType.GinMerge, true),
        new(ExtendedSearchType.GinMergeFilter),
        new(ExtendedSearchType.GinMergeFilter, true),
        new(ExtendedSearchType.GinOffset),
        new(ExtendedSearchType.GinOffset, true),
        new(ExtendedSearchType.GinOffsetFilter),
        new(ExtendedSearchType.GinOffsetFilter, true),
        new(ExtendedSearchType.GinDirectOffset),
        new(ExtendedSearchType.GinDirectOffset, true),
        new(ExtendedSearchType.GinDirectOffsetFilter),
        new(ExtendedSearchType.GinDirectOffsetFilter, true),
        new(ExtendedSearchType.GinFrozenDirect),
        new(ExtendedSearchType.GinFrozenDirect, true),
        new(ExtendedSearchType.GinFrozenDirectFilter),
        new(ExtendedSearchType.GinFrozenDirectFilter, true)
    ];

    public static List<string> SearchQueries =>
    [
        //"пляшем на столе за детей",
        "удача с ними за столом",
        //"чорт з ным зо сталом",
        "чёрт с ними за столом",
        "с ними за столом чёрт",
        //"преключиться вдруг верный друг",
        "приключится вдруг верный друг",
        "приключится вдруг вот верный друг выручить",
        "пляшем на",
        "ты шла по палубе в молчаний",
        //"оно шла по палубе в молчаний",
        //"123 456 иии",
        //"aa bb cc dd .,/#",
        //" |",
        "я ты он она",
        "a b c d .,/#",
        //" ",
        "на",
        "b b b b b b",
        "b b b b b",
        "b b b b",
        "b"
    ];

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public required BenchmarkParameter<ExtendedSearchType> SearchType;

    /*[ParamsSource(nameof(SearchQueries))]
    // ReSharper disable once UnassignedField.Global
    public required string SearchQuery;*/

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        await InitializeTokenizer(SearchType.SearchType, SearchType.Pool);
    }

    [Benchmark]
    public void QueryExtended()
    {
        var counter = 0;

        for (;;)
        {
            for (var i = 0; i < SearchQueries.Count; i++)
            {
                counter++;

                if (counter > QueriesCount)
                {
                    return;
                }

                var index = i;

                var searchQuery = SearchQueries[index];
                //var searchQuery = SearchQuery;

                var result = _tokenizer.ComputeComplianceIndexExtended(searchQuery, CancellationToken.None);
                if (result.Count == 0)
                {
                    Console.WriteLine("Result is empty [" + searchQuery + "]");
                }
            }
        }
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        QueryExtended();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(Constants.TokenizerExtendedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(StQueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(pool, extendedSearchType, ReducedSearchType.Legacy);

        Console.WriteLine(
            $"[{nameof(StQueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(StQueryBenchmarkExtended)}] extended[{extendedSearchType}] initialized '{result:N0}' vectors.");
    }
}
