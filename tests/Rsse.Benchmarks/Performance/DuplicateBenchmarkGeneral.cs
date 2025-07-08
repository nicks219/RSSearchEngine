using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Rsse.Domain.Data.Entities;
using Rsse.Tests.Common;
using RsseEngine.SearchType;
using RsseEngine.Service;
using static RsseEngine.Benchmarks.Constants;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск дубликатов во всех документах, используя extended/reduced алгоритмы.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
public class DuplicateBenchmarkGeneral : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    private List<NoteEntity> _noteEntities = null!;

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
    public void DuplicatesExtendedAndReduced()
    {
        foreach (NoteEntity noteEntity in _noteEntities)
        {
            var results = _tokenizer.ComputeComplianceIndices(noteEntity.Text, CancellationToken.None);
            if (results.Count == 0)
            {
                Console.WriteLine("[Tokenizer] empty result");
            }
        }

        // Console.WriteLine($"[{nameof(BenchmarkEngineTokenizer)}] found: {results.Count}");
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        DuplicatesExtendedAndReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerExtendedSearchType, TokenizerReducedSearchType);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        Console.WriteLine(
            $"[{nameof(DuplicateBenchmarkGeneral)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(false, extendedSearchType, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataMultipleProvider(1);

        _noteEntities = [];

        await foreach (var noteEntity in dataProvider.GetDataAsync())
        {
            _noteEntities.Add(noteEntity);
        }

        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
