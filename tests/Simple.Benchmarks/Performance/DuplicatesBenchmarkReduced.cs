using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Domain.Data.Entities;
using Rsse.Tests.Common;
using SimpleEngine.Benchmarks.Common;
using SimpleEngine.SearchType;
using SimpleEngine.Service;

namespace SimpleEngine.Benchmarks.Performance;

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
        new(ReducedSearchType.SimpleLegacy),
        new(ReducedSearchType.Direct),
        new(ReducedSearchType.Direct, true),
        //new(ReducedSearchType.MergeFilter),
        //new(ReducedSearchType.MergeFilter, true),
        //new(ReducedSearchType.DirectFilterLinear),
        //new(ReducedSearchType.DirectFilterLinear, true),
        //new(ReducedSearchType.DirectFilterBinary),
        //new(ReducedSearchType.DirectFilterBinary, true),
        //new(ReducedSearchType.DirectFilterHash),
        //new(ReducedSearchType.DirectFilterHash, true)
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
            var metricsCalculator = _tokenizer.CreateMetricsCalculator();

            try
            {
                _tokenizer.ComputeComplianceIndexReduced(noteEntity.Text,
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
        DuplicatesReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(Constants.TokenizerReducedSearchType, false);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ReducedSearchType reducedSearchType, bool pool)
    {
        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(MetricsCalculatorType.NoOpMetricsCalculator,
            pool, ExtendedSearchType.Legacy, reducedSearchType);

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
