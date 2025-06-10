using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SearchEngine.Benchmarks.Common;
using SearchEngine.Service.Tokenizer;
using SearchEngine.Service.Tokenizer.Factory;

namespace SearchEngine.Benchmarks;

public class TokenizerBenchmarks
{
    private static readonly SearchEngineTokenizer Tokenizer;

    // private const string Text = "пляшем на столе за детей";
    private const string Text = "преключиться вдруг верный друг";
    // private const string Text = "приключится вдруг верный друг";
    private static bool _isInitialized;

    static TokenizerBenchmarks()
    {
        var processorFactory = new TokenizerProcessorFactory();
        Tokenizer = new SearchEngineTokenizer(processorFactory, SearchType.GinOptimized);
    }

    [GlobalSetup]
    public static async Task SetupAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        await InitializeEngineTokenizer();

        await InitializeLucene();
    }

    [Benchmark]
    public void BenchmarkEngineTokenizer()
    {
        var results = Tokenizer.ComputeComplianceIndices(Text, CancellationToken.None);
        if (results.Count == 0)
        {
            Console.WriteLine("TOKENIZER: EMPTY RESULTS");
        }

        // Console.WriteLine($"[{nameof(BenchmarkEngineTokenizer)}] found: {results.Count}");
    }

    [Benchmark]
    public void BenchmarkLucene()
    {
        var result = LuceneWrapper.Find(Text);

        if (result.Count == 0)
        {
            Console.WriteLine("LUCENE: EMPTY RESULTS");
        }

        // Console.WriteLine($"[{nameof(BenchmarkLucene)}] found: {result.Count}");
    }

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    internal static async Task InitializeEngineTokenizer()
    {
        Console.WriteLine($"[{nameof(SearchEngineTokenizer)}] initializing..");

        var dataProvider = new FileDataProvider();
        var result = await Tokenizer.InitializeAsync(dataProvider, CancellationToken.None);
        Console.WriteLine($"[{nameof(SearchEngineTokenizer)}] initialized '{result:N0}' vectors");
    }

    /// <summary>
    /// Инициализировать Lucene.
    /// </summary>
    private static async Task InitializeLucene()
    {
        Console.WriteLine($"[{nameof(LuceneWrapper)}] initializing..");
        await LuceneWrapper.InitializeAsync();
    }
}
