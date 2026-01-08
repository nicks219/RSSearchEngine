using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using RD.RsseEngine.Benchmarks.Common;
using RD.RsseEngine.Service;
using Rsse.Tests.Common;
using RD.RsseEngine.SearchType;

namespace RD.RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск тестового запроса во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class StQueryBenchmarkReduced : IBenchmarkRunner
{
    private const int QueriesCount = 1000;

    private TokenizerServiceCore _tokenizer = null!;

    public static List<BenchmarkParameter<ReducedSearchType>> Parameters =>
    [
        new(ReducedSearchType.Legacy),
        new(ReducedSearchType.GinArrayDirect),
        new(ReducedSearchType.GinArrayDirect, true),
        new(ReducedSearchType.GinArrayMergeFilter),
        new(ReducedSearchType.GinArrayMergeFilter, true),
        new(ReducedSearchType.GinArrayDirectFilterLs),
        new(ReducedSearchType.GinArrayDirectFilterLs, true),
        new(ReducedSearchType.GinArrayDirectFilterBs),
        new(ReducedSearchType.GinArrayDirectFilterBs, true),
        new(ReducedSearchType.GinArrayDirectFilterHs),
        new(ReducedSearchType.GinArrayDirectFilterHs, true)
    ];

    public static List<string> SearchQueries =>
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
    public required BenchmarkParameter<ReducedSearchType> SearchType;

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
    public void QueryReduced()
    {
        var counter = 0;

        for (; ; )
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

                var metricsCalculator = _tokenizer.CreateMetricsCalculator();

                try
                {
                    _tokenizer.ComputeComplianceIndexReduced(searchQuery, metricsCalculator, CancellationToken.None);

                    if (metricsCalculator.ComplianceMetrics.Count == 0)
                    {
                        Console.WriteLine("Result is empty [" + searchQuery + "]");
                    }
                }
                finally
                {
                    _tokenizer.ReleaseMetricsCalculator(metricsCalculator);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        QueryReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(Constants.TokenizerReducedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ReducedSearchType reducedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(StQueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(MetricsCalculatorType.NoOpMetricsCalculator,
            pool, ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(StQueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(StQueryBenchmarkReduced)}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
