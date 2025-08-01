using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Rsse.Tests.Common;
using RsseEngine.SearchType;
using RsseEngine.Service;

namespace RsseEngine.Benchmarks.Performance;

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
        (Extended: ExtendedSearchType.GinSimple, Reduced: ReducedSearchType.GinSimple),
        (Extended: ExtendedSearchType.GinOptimized, Reduced: ReducedSearchType.GinOptimized),
        (Extended: ExtendedSearchType.GinOptimized, Reduced: ReducedSearchType.GinOptimizedFilter),
        (Extended: ExtendedSearchType.GinFilter, Reduced:ReducedSearchType.GinFilter),
        (Extended: ExtendedSearchType.GinFast, Reduced:ReducedSearchType.GinFast),
        (Extended: ExtendedSearchType.GinFastFilter, Reduced:ReducedSearchType.GinFastFilter)
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
        var results = _tokenizer.ComputeComplianceIndices(Constants.SearchQuery, CancellationToken.None);
        if (results.Count == 0)
        {
            Console.WriteLine("[Tokenizer] empty result");
        }

        // Console.WriteLine($"[{nameof(BenchmarkEngineTokenizer)}] found: {results.Count}");
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

        _tokenizer = new TokenizerServiceCore(false, extendedSearchType, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
