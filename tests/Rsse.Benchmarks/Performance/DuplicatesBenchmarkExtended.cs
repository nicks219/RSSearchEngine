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
using RsseEngine.Selector;
using RsseEngine.Service;

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
        new(ExtendedSearchType.GinOffset),
        new(ExtendedSearchType.GinOffset, true),
        new(ExtendedSearchType.GinOffsetFilter),
        new(ExtendedSearchType.GinOffsetFilter, true),
        new(ExtendedSearchType.GinArrayDirectLs),
        new(ExtendedSearchType.GinArrayDirectLs, true),
        new(ExtendedSearchType.GinArrayDirectFilterLs),
        new(ExtendedSearchType.GinArrayDirectFilterLs, true),
        new(ExtendedSearchType.GinArrayDirectBs),
        new(ExtendedSearchType.GinArrayDirectBs, true),
        new(ExtendedSearchType.GinArrayDirectFilterBs),
        new(ExtendedSearchType.GinArrayDirectFilterBs, true),
        new(ExtendedSearchType.GinArrayDirectHs),
        new(ExtendedSearchType.GinArrayDirectHs, true),
        new(ExtendedSearchType.GinArrayDirectFilterHs),
        new(ExtendedSearchType.GinArrayDirectFilterHs, true)
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
            var metricsCalculator = _tokenizer.CreateMetricsCalculator();

            try
            {
                _tokenizer.ComputeComplianceIndexExtended(noteEntity.Text,
                    metricsCalculator, CancellationToken.None);

                if (metricsCalculator.ComplianceMetrics.Count == 0)
                {
                    Console.WriteLine("[Tokenizer] empty result");
                }
            }
            finally
            {
                _tokenizer.ReleaseMetricsCalculator(metricsCalculator);
            }
        }
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        DuplicatesExtended();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(Constants.TokenizerExtendedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ExtendedSearchType extendedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkExtended)}] extended[{extendedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(SearchIndexTypeSelector.GetIndexType(extendedSearchType),
            MetricsCalculatorType.NoOpMetricsCalculator, pool, extendedSearchType, ReducedSearchType.Legacy);

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
