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
public class QueryBenchmarkExtended : IBenchmarkRunner
{
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
        new(ExtendedSearchType.GinMergeFilter, true)
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
    public Dictionary<DocumentId, double> QueryExtended()
    {
        return _tokenizer.ComputeComplianceIndexExtended(SearchQuery, CancellationToken.None);
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        QueryExtended();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerExtendedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(pool, extendedSearchType, ReducedSearchType.Legacy);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initialized '{result:N0}' vectors.");
    }
}
