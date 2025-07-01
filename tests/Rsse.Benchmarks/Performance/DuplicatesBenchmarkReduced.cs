using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Domain.Data.Entities;
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

    public static List<ReducedSearchType> Parameters =>
        ((ReducedSearchType[])Enum.GetValuesAsUnderlyingType<ReducedSearchType>())
        .ToList();

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public ReducedSearchType SearchType;

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        await InitializeTokenizer(SearchType);
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
    public Task Initialize() => InitializeTokenizer(TokenizerReducedSearchType);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ReducedSearchType reducedSearchType)
    {
        Console.WriteLine(
            $"[{nameof(DuplicatesBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(TokenizerServiceCore)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataProvider(1);

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
