using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Domain.Data.Entities;
using Rsse.Tests.Common;
using RsseEngine.Benchmarks.Common;
using RsseEngine.SearchType;
using RsseEngine.Service;
using static RsseEngine.Benchmarks.Constants;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск дубликатов во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class DuplicatesBenchmarkExtended : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    private List<NoteEntity> _noteEntities = null!;

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
        new(ExtendedSearchType.GinMergeFilter, true),
        new(ExtendedSearchType.GinOffset),
        new(ExtendedSearchType.GinOffset, true),
        new(ExtendedSearchType.GinOffsetFilter),
        new(ExtendedSearchType.GinOffsetFilter, true)
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
    public void DuplicatesExtended()
    {
        foreach (NoteEntity noteEntity in _noteEntities)
        {
            var results = _tokenizer.ComputeComplianceIndexExtended(noteEntity.Text, CancellationToken.None);
            if (results.Count == 0)
            {
                Console.WriteLine("[Tokenizer] empty result");
            }
        }
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        DuplicatesExtended();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerExtendedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(pool, extendedSearchType, ReducedSearchType.Legacy);

        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider(1);

        _noteEntities = new List<NoteEntity>();

        await foreach (NoteEntity noteEntity in dataProvider.GetDataAsync())
        {
            _noteEntities.Add(noteEntity);
        }

        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkExtended)}] extended[{extendedSearchType}] initialized '{result:N0}' vectors.");
    }
}
