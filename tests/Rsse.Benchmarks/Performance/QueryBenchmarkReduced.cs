using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Tests.Common;
using RsseEngine.Benchmarks.Common;
using RsseEngine.Dto;
using RsseEngine.SearchType;
using RsseEngine.Service;
using static RsseEngine.Benchmarks.Constants;

namespace RsseEngine.Benchmarks.Performance;

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
        new(ReducedSearchType.Legacy),
        new(ReducedSearchType.GinSimple),
        new(ReducedSearchType.GinOptimized),
        new(ReducedSearchType.GinOptimized, true),
        new(ReducedSearchType.GinOptimizedFilter),
        new(ReducedSearchType.GinOptimizedFilter, true),
        new(ReducedSearchType.GinFilter),
        new(ReducedSearchType.GinFilter, true),
        new(ReducedSearchType.GinFast),
        new(ReducedSearchType.GinFast, true),
        new(ReducedSearchType.GinFastFilter),
        new(ReducedSearchType.GinFastFilter, true)
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
    public Dictionary<DocumentId, double> QueryReduced()
    {
        return _tokenizer.ComputeComplianceIndexReduced(SearchQuery, CancellationToken.None);
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        QueryReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerReducedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ReducedSearchType reducedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(pool, ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
