using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SearchEngine.Api.Services;
using SearchEngine.Benchmarks.Common;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Tokenizer;

namespace SearchEngine.Benchmarks;

[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 3)]
// [ShortRunJob(RuntimeMoniker.Net90)]
[MinColumn, MaxColumn, MeanColumn]
[AsciiDocExporter]
public class TokenizerBenchmark
{
    private const string Text = "преключиться вдруг верный друг";
    // private const string Text = "приключится вдруг верный друг";
    private static TokenizerService? _tokenizer;

    [GlobalSetup]
    public static async Task Setup()
    {
        Console.WriteLine($"[{nameof(TokenizerService)}] initializing..");
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });

        var processorFactory = new TokenizerProcessorFactory();
        var loggerFactory = LoggerFactory.Create(o =>
        {
            o.AddConsole();
        });

        var logger = loggerFactory.CreateLogger<TokenizerService>();

        _tokenizer = new TokenizerService(
            processorFactory,
            options,
            logger);

        var dataProvider = new FileDataProvider();
        await _tokenizer.Initialize(dataProvider, CancellationToken.None);

        Console.WriteLine($"[{nameof(LuceneTokenizer)}] initializing..");
        await LuceneTokenizer.InitializeLucene();
    }

    [Benchmark]
    public void TokenizersBenchmark()
    {
        var results = _tokenizer?.ComputeComplianceIndices(Text, CancellationToken.None);
        if (results == null || results.Count == 0)
        {
            Console.WriteLine("TOKENIZER: EMPTY RESULTS");
        }

        // foreach (var result in results)
        // {
        //    Console.WriteLine($"TOKENIZER: {result.Key} --- {result.Value}");
        // }
    }

    [Benchmark]
    public void LuceneBenchmark()
    {
        foreach (var result in LuceneTokenizer.Find(Text))
        {
            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine("LUCENE: EMPTY RESULTS");
            }

            // Console.WriteLine($"LUCENE: {r.Length}");
        }
    }
}
