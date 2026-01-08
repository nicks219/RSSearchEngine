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
public class QueryBenchmarkExtended : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    public static List<BenchmarkParameter<ExtendedSearchType>> Parameters =>
    [
        new(ExtendedSearchType.Legacy),
        new(ExtendedSearchType.GinOffset),
        new(ExtendedSearchType.GinOffset, true),
        new(ExtendedSearchType.GinOffsetFilter),
        new(ExtendedSearchType.GinOffsetFilter, true),
        new(ExtendedSearchType.GinArrayDirectLs),
        new(ExtendedSearchType.GinArrayDirectLs, true),
        new(ExtendedSearchType.GinArrayDirectFilterLs),
        new(ExtendedSearchType.GinArrayDirectFilterLs, true),
        new(ExtendedSearchType.GinArrayDirectBs),
        new(ExtendedSearchType.GinArrayDirectBs, true),
        new(ExtendedSearchType.GinArrayDirectFilterBs),
        new(ExtendedSearchType.GinArrayDirectFilterBs, true),
        new(ExtendedSearchType.GinArrayDirectHs),
        new(ExtendedSearchType.GinArrayDirectHs, true),
        new(ExtendedSearchType.GinArrayDirectFilterHs),
        new(ExtendedSearchType.GinArrayDirectFilterHs, true)
    ];

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public required BenchmarkParameter<ExtendedSearchType> SearchType;

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
        var metricsCalculator = _tokenizer.CreateMetricsCalculator();

        try
        {
            _tokenizer.ComputeComplianceIndexExtended(Constants.SearchQuery,
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
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(MetricsCalculatorType.NoOpMetricsCalculator,
            pool, extendedSearchType, ReducedSearchType.Legacy);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initialized '{result:N0}' vectors.");
    }
}
