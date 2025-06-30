using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using RsseEngine.Benchmarks.Common;
using RsseEngine.SearchType;
using RsseEngine.Service;
using static RsseEngine.Benchmarks.Constants;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск тестового запроса во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
public class QueryBenchmark : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    public static IEnumerable<(ExtendedSearchType Extended, ReducedSearchType Reduced)> Parameters =>
    [
        (Extended: ExtendedSearchType.Legacy, Reduced: ReducedSearchType.Legacy),
        /*(ExtendedSearchType.Legacy, ReducedSearchType.GinSimple),
        (ExtendedSearchType.Legacy, ReducedSearchType.GinOptimized),
        (ExtendedSearchType.Legacy, ReducedSearchType.GinFast),
        (ExtendedSearchType.GinSimple, ReducedSearchType.Legacy),*/
        (Extended: ExtendedSearchType.GinSimple, Reduced: ReducedSearchType.GinSimple),
        /*(ExtendedSearchType.GinSimple, ReducedSearchType.GinOptimized),
        (ExtendedSearchType.GinSimple, ReducedSearchType.GinFast),
        (ExtendedSearchType.GinOptimized, ReducedSearchType.Legacy),
        (ExtendedSearchType.GinOptimized, ReducedSearchType.GinSimple),*/
        (Extended: ExtendedSearchType.GinOptimized, Reduced: ReducedSearchType.GinOptimized),
        /*(ExtendedSearchType.GinOptimized, ReducedSearchType.GinFast),
        (ExtendedSearchType.GinFast, ReducedSearchType.Legacy),
        (ExtendedSearchType.GinFast, ReducedSearchType.GinSimple),
        (ExtendedSearchType.GinFast, ReducedSearchType.GinOptimized),*/
        (Extended: ExtendedSearchType.GinFast, Reduced: ReducedSearchType.GinFast)
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
    public void FindSentence()
    {
        var results = _tokenizer.ComputeComplianceIndices(SearchQuery, CancellationToken.None);
        if (results.Count == 0)
        {
            Console.WriteLine("[Tokenizer] empty result");
        }

        // Console.WriteLine($"[{nameof(BenchmarkEngineTokenizer)}] found: {results.Count}");
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        FindSentence();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerExtendedSearchType, TokenizerReducedSearchType);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        Console.WriteLine(
            $"[{nameof(QueryBenchmark)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(extendedSearchType, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
