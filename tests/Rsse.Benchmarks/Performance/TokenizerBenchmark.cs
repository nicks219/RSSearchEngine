using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SearchEngine.Benchmarks.Common;
using SearchEngine.Service.Tokenizer;
using static SearchEngine.Benchmarks.Constants;

namespace SearchEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на Tokenizer.
/// </summary>
[MinColumn]
public class TokenizerBenchmark : IBenchmarkRunner
{
    private static readonly SearchEngineTokenizer Tokenizer;
    private static bool _isInitialized;

    static TokenizerBenchmark()
    {
        var processorFactory = new TokenizerProcessorFactory();
        Tokenizer = new SearchEngineTokenizer(processorFactory, TokenizerSearchType);
    }

    [GlobalSetup]
    public static async Task SetupAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        Console.WriteLine($"[{nameof(TokenizerBenchmark)}] initializing..");

        await InitializeEngineTokenizer();
    }

    /// <inheritdoc/>
    [Benchmark]
    public void RunBenchmark()
    {
        var results = Tokenizer.ComputeComplianceIndices(SearchQuery, CancellationToken.None);
        if (results.Count == 0)
        {
            Console.WriteLine("[Tokenizer] empty result");
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
        Console.WriteLine($"[{nameof(SearchEngineTokenizer)}] initializing..");

        var dataProvider = new FileDataProvider();
        var result = await Tokenizer.InitializeAsync(dataProvider, CancellationToken.None);
        Console.WriteLine($"[{nameof(SearchEngineTokenizer)}] initialized '{result:N0}' vectors.");
    }
}
