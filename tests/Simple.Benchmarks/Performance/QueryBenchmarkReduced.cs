using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Tests.Common;
using SimpleEngine.Benchmarks.Common;
using SimpleEngine.SearchType;
using SimpleEngine.Service;

namespace SimpleEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск тестового запроса во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class QueryBenchmarkReduced : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    public static List<BenchmarkParameter<ReducedSearchType>> Parameters =>
    [
        //new(ReducedSearchType.Legacy),
        new(ReducedSearchType.SimpleLegacy),
        new(ReducedSearchType.SimpleLegacy, true),
        new(ReducedSearchType.Direct),
        new(ReducedSearchType.Direct, true),
        //new(ReducedSearchType.MergeFilter),
        //new(ReducedSearchType.MergeFilter, true),
        //new(ReducedSearchType.DirectFilterLinear),
        //new(ReducedSearchType.DirectFilterLinear, true),
        //new(ReducedSearchType.DirectFilterBinary),
        //new(ReducedSearchType.DirectFilterBinary, true),
        //new(ReducedSearchType.DirectFilterHash),
        //new(ReducedSearchType.DirectFilterHash, true)
    ];

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public required BenchmarkParameter<ReducedSearchType> SearchType;

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
        var metricsCalculator = _tokenizer.CreateMetricsCalculator();

        try
        {
            _tokenizer.ComputeComplianceIndexReduced(Constants.SearchQuery,
                metricsCalculator, CancellationToken.None);

            if (metricsCalculator.ComplianceMetrics.Count == 0)
            {
                Console.WriteLine("Result is empty [" + Constants.SearchQuery + "]");
            }
        }
        finally
        {
            _tokenizer.ReleaseMetricsCalculator(metricsCalculator);
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
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(MetricsCalculatorType.NoOpMetricsCalculator,
            pool, ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
