using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using RD.RsseEngine.SearchType;
using RD.RsseEngine.Service;
using Rsse.Tests.Common;

namespace RD.RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск тестового запроса во всех документах, используя extended/reduced алгоритмы.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
public class QueryBenchmarkGeneral : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    public static IEnumerable<(ExtendedSearchType Extended, ReducedSearchType Reduced)> Parameters =>
    [
        (Extended: ExtendedSearchType.Legacy, Reduced: ReducedSearchType.Legacy),
        (Extended: ExtendedSearchType.GinArrayDirectFilterLs, Reduced:ReducedSearchType.GinArrayDirectFilterLs)
    ];

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public (ExtendedSearchType Extended, ReducedSearchType Reduced) SearchType;

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        await InitializeTokenizer(SearchType.Extended, SearchType.Reduced);
    }

    [Benchmark]
    public void QueryExtendedAndReduced()
    {
        var metricsCalculator = _tokenizer.CreateMetricsCalculator();

        try
        {
            _tokenizer.ComputeComplianceIndices(Constants.SearchQuery,
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
        QueryExtendedAndReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(Constants.TokenizerExtendedSearchType, Constants.TokenizerReducedSearchType);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        Console.WriteLine(
            $"[{nameof(QueryBenchmarkGeneral)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(MetricsCalculatorType.NoOpMetricsCalculator,
            false, extendedSearchType, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
