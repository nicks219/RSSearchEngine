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
        new(ExtendedSearchType.GinFilter),
        new(ExtendedSearchType.GinFilter, true),
        new(ExtendedSearchType.GinFast),
        new(ExtendedSearchType.GinFast, true),
        new(ExtendedSearchType.GinFastFilter),
        new(ExtendedSearchType.GinFastFilter, true),
        new(ExtendedSearchType.GinMerge),
        new(ExtendedSearchType.GinMerge, true),
        new(ExtendedSearchType.GinMergeFilter),
        new(ExtendedSearchType.GinMergeFilter, true),
        new(ExtendedSearchType.GinOffset),
        new(ExtendedSearchType.GinOffset, true),
        new(ExtendedSearchType.GinOffsetFilter),
        new(ExtendedSearchType.GinOffsetFilter, true),
        new(ExtendedSearchType.GinDirectOffset),
        new(ExtendedSearchType.GinDirectOffset, true),
        new(ExtendedSearchType.GinDirectOffsetFilter),
        new(ExtendedSearchType.GinDirectOffsetFilter, true),
        new(ExtendedSearchType.GinFrozenDirect),
        new(ExtendedSearchType.GinFrozenDirect, true),
        new(ExtendedSearchType.GinFrozenDirectFilter),
        new(ExtendedSearchType.GinFrozenDirectFilter, true)
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
        return _tokenizer.ComputeComplianceIndexExtended(Constants.SearchQuery, CancellationToken.None);
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

        _tokenizer = new TokenizerServiceCore(pool, extendedSearchType, ReducedSearchType.Legacy);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkExtended)}] extended[{extendedSearchType}] initialized '{result:N0}' vectors.");
    }
}
