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
public class DuplicatesBenchmarkReduced : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    private List<NoteEntity> _noteEntities = null!;

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
        new(ReducedSearchType.GinFastFilter, true),
        new(ReducedSearchType.GinMerge1),
        new(ReducedSearchType.GinMerge1, true),
        new(ReducedSearchType.GinMergeFilter1),
        new(ReducedSearchType.GinMergeFilter1, true),
        new(ReducedSearchType.GinMerge2),
        new(ReducedSearchType.GinMerge2, true),
        new(ReducedSearchType.GinMergeFilter2),
        new(ReducedSearchType.GinMergeFilter2, true)
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
    public void DuplicatesReduced()
    {
        foreach (NoteEntity noteEntity in _noteEntities)
        {
            var results = _tokenizer.ComputeComplianceIndexReduced(noteEntity.Text, CancellationToken.None);
            if (results.Count == 0)
            {
                Console.WriteLine("[Tokenizer] empty result");
            }
        }
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        DuplicatesReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerReducedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ReducedSearchType reducedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(pool, ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider(1);

        _noteEntities = new List<NoteEntity>();

        await foreach (NoteEntity noteEntity in dataProvider.GetDataAsync())
        {
            _noteEntities.Add(noteEntity);
        }

        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
