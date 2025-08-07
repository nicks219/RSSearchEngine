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
public class MtQueryBenchmarkReduced : IBenchmarkRunner
{
    private const int QueriesCount = 1000;

    private readonly List<Task> _tasks = new(QueriesCount);

    private TokenizerServiceCore _tokenizer = null!;

    public static List<BenchmarkParameter<ReducedSearchType>> Parameters =>
    [
        new(ReducedSearchType.Legacy),
        new(ReducedSearchType.GinOptimized),
        new(ReducedSearchType.GinOptimized, true),
        new(ReducedSearchType.GinOptimizedFilter),
        new(ReducedSearchType.GinOptimizedFilter, true),
        new(ReducedSearchType.GinFilter),
        new(ReducedSearchType.GinFilter, true),
        new(ReducedSearchType.GinFast),
        new(ReducedSearchType.GinFast, true),
        new(ReducedSearchType.GinFastFilter),
        new(ReducedSearchType.GinFastFilter, true),
        new(ReducedSearchType.GinMerge),
        new(ReducedSearchType.GinMerge, true),
        new(ReducedSearchType.GinMergeFilter),
        new(ReducedSearchType.GinMergeFilter, true)
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
        _tasks.Clear();
        var counter = 0;

        for (;;)
        {
            for (var i = 0; i < SearchQueries.Count; i++)
            {
                counter++;

                if (counter > QueriesCount)
                {
                    Task.WaitAll(_tasks);
                    return;
                }

                var index = i;

                _tasks.Add(Task.Run(() =>
                {
                    var searchQuery = SearchQueries[index];
                    //var searchQuery = SearchQuery;

                    var result = _tokenizer.ComputeComplianceIndexReduced(searchQuery, CancellationToken.None);
                    if (result.Count == 0)
                    {
                        Console.WriteLine("Result is empty [" + searchQuery + "]");
                    }
                }));
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
            $"[{nameof(MtQueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(pool, ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(MtQueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(MtQueryBenchmarkReduced)}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
