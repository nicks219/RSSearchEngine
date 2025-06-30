using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using RsseEngine;
using RsseEngine.SearchType;
using SearchEngine.Benchmarks.Common;
using SearchEngine.Data.Entities;
using static SearchEngine.Benchmarks.Constants;

namespace SearchEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск дубликатов во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
public class DuplicatesBenchmark : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    private List<NoteEntity> _noteEntities = null!;

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
    public void FindText()
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
        FindText();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerExtendedSearchType, TokenizerReducedSearchType);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmark)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(extendedSearchType, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataProvider(1);

        _noteEntities = new List<NoteEntity>();

        await foreach (NoteEntity noteEntity in dataProvider.GetDataAsync())
        {
            _noteEntities.Add(noteEntity);
        }

        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] extended[{extendedSearchType}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
