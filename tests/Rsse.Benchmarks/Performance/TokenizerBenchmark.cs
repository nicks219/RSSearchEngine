using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Rsse.Search;
using SearchEngine.Benchmarks.Common;
using SearchEngine.Service.Tokenizer;
using static SearchEngine.Benchmarks.Constants;

namespace SearchEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на Tokenizer.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
public class TokenizerBenchmark : IBenchmarkRunner
{
    private SearchEngineTokenizer _tokenizer;

    [ParamsSource(nameof(Parameters))]
    public (ExtendedSearchType extended, ReducedSearchType reduced) SearchType;

    public IEnumerable<(ExtendedSearchType extended, ReducedSearchType reduced)> Parameters =>
    [
        (ExtendedSearchType.Original, ReducedSearchType.Original),
        /*(ExtendedSearchType.Original, ReducedSearchType.GinSimple),
        (ExtendedSearchType.Original, ReducedSearchType.GinOptimized),
        (ExtendedSearchType.Original, ReducedSearchType.GinFast),
        (ExtendedSearchType.GinSimple, ReducedSearchType.Original),*/
        (ExtendedSearchType.GinSimple, ReducedSearchType.GinSimple),
        /*(ExtendedSearchType.GinSimple, ReducedSearchType.GinOptimized),
        (ExtendedSearchType.GinSimple, ReducedSearchType.GinFast),
        (ExtendedSearchType.GinOptimized, ReducedSearchType.Original),
        (ExtendedSearchType.GinOptimized, ReducedSearchType.GinSimple),*/
        (ExtendedSearchType.GinOptimized, ReducedSearchType.GinOptimized),
        /*(ExtendedSearchType.GinOptimized, ReducedSearchType.GinFast),
        (ExtendedSearchType.GinFast, ReducedSearchType.Original),
        (ExtendedSearchType.GinFast, ReducedSearchType.GinSimple),
        (ExtendedSearchType.GinFast, ReducedSearchType.GinOptimized),*/
        (ExtendedSearchType.GinFast, ReducedSearchType.GinFast)
    ];

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        await InitializeTokenizer(SearchType.extended, SearchType.reduced);
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
            $"[{nameof(TokenizerBenchmark)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var processorFactory = new TokenizerProcessorFactory();
        _tokenizer = new SearchEngineTokenizer(processorFactory, extendedSearchType, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(SearchEngineTokenizer)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(SearchEngineTokenizer)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
