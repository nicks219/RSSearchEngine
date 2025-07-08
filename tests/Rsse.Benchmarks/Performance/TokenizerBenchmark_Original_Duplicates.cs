using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Rsse.Domain.Data.Entities;
using Rsse.Tests.Common;
using RsseEngine.Dto;
using RsseEngine.Service;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на Tokenizer.
/// </summary>
[MinColumn]
public class TokenizerBenchmark_Original_Duplicates : IBenchmarkRunner
{
    private static TokenizerServiceCore Tokenizer = null!;
    private static bool _isInitialized;

    static TokenizerBenchmark_Original_Duplicates()
    {
        Tokenizer = new TokenizerServiceCore(MetricsCalculatorType.PooledMetricsCalculator, false);
    }

    [GlobalSetup]
    public static async Task SetupAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        Console.WriteLine($"[{nameof(QueryBenchmarkGeneral)}] initializing..");

        await InitializeEngineTokenizer();
    }

    /// <inheritdoc/>
    [Benchmark]
    public void RunBenchmark()
    {
        foreach (NoteEntity noteEntity in _noteEntities/*.TakeEx()*/)
        {
            var metricsCalculator = Tokenizer.CreateMetricsCalculator();

            try
            {
                Tokenizer.ComputeComplianceIndices(noteEntity.Text, metricsCalculator, CancellationToken.None);
                if (metricsCalculator.ComplianceMetrics.Count > 1)
                {
                    Console.WriteLine("[Tokenizer] empty result [" + metricsCalculator.ComplianceMetrics.Count + "]");
                    foreach (KeyValuePair<DocumentId, double> result in metricsCalculator.ComplianceMetrics)
                    {
                        NoteEntity entity = _noteEntities.Where(t => t.NoteId == result.Key.Value /*&& t.NoteId != noteEntity.NoteId*/).First();
                        Console.WriteLine("                             [" + entity.NoteId + "   " + entity.Title + "] + [" + result.Value + "]");
                    }

                    Console.WriteLine();
                }
            }
            finally
            {
                Tokenizer.ReleaseMetricsCalculator(metricsCalculator);
            }
        }

        // Console.WriteLine($"[{nameof(BenchmarkEngineTokenizer)}] found: {results.Count}");
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeEngineTokenizer();

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private static async Task InitializeEngineTokenizer()
    {
        Console.WriteLine($"[{nameof(TokenizerServiceCore)}] initializing..");

        var dataProvider = new FileDataMultipleProvider(1);

        _noteEntities = new List<NoteEntity>();
        await foreach (NoteEntity noteEntity in dataProvider.GetDataAsync())
        {
            _noteEntities.Add(noteEntity);
        }

        var result = await Tokenizer.InitializeAsync(dataProvider, CancellationToken.None);
        Console.WriteLine($"[{nameof(TokenizerServiceCore)}] initialized '{result:N0}' vectors.");
    }

    private static List<NoteEntity> _noteEntities;
}
