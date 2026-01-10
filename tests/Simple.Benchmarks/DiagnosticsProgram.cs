using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using Perfolizer.Metrology;
using SimpleEngine.Benchmarks.Common;
using SimpleEngine.Benchmarks.Performance;
using SimpleEngine.Benchmarks.Validation;
using static SimpleEngine.Benchmarks.Constants;

namespace SimpleEngine.Benchmarks;

/// <summary>
/// Бенчмарки и профилирование.
/// </summary>
public static class DiagnosticsProgram
{
    /// <summary>
    /// Выбор между режимами измерения и профилирования производительности.
    /// </summary>
    /// <param name="args">Выбор режима: "bench"/"profile".</param>
    public static async Task Main(string[] args)
    {
        var isGitHubCi = Environment.GetEnvironmentVariable("DOTNET_CI");
        if (isGitHubCi == "true")
        {
            Console.WriteLine($"[{nameof(DiagnosticsProgram)}] benchmark runner skipped on CI");
            Environment.Exit(1);
        }

        if (args.Length != 2)
        {
            Console.WriteLine($"[{nameof(DiagnosticsProgram)}] invalid args, usage: <bench>|<profile>");
            Console.WriteLine("Run benchmarks...");
            RunBenchmarks(string.Empty);
            Environment.Exit(0);
        }

        var argFirst = args[0];
        var argSecond = args[1];
        switch (argFirst)
        {
            case "bench":
                RunBenchmarks(argSecond);
                break;

            case "profile":
                await RunProfiling();
                break;

            default:
                RunBenchmarks(argSecond);
                break;
        }
    }

    /// <summary>
    /// Запустить бенчмарки.
    /// </summary>
    private static void RunBenchmarks(string arg)
    {
        Console.WriteLine($"[{nameof(RunBenchmarks)}] starting..");

        // Запуск бенчмарков с возможностью дебага, либо без неё.
        var config = Debugger.IsAttached
            ? new DebugInProcessConfig().WithOptions(ConfigOptions.DisableOptimizationsValidator)
            : DefaultConfig.Instance
                .WithSummaryStyle(DefaultConfig.Instance.SummaryStyle
                    .WithMaxParameterColumnWidth(100)
                    .WithSizeUnit(SizeUnit.KB)
                    .WithTimeUnit(TimeUnit.Millisecond))
                .WithOptions(ConfigOptions.DisableOptimizationsValidator | ConfigOptions.JoinSummary)
                .AddJob(Job.VeryLongRun
                    .WithWarmupCount(1)
                    .WithLaunchCount(1)
                    .WithIterationCount(5))// 10
                .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(displayGenColumns: false)))
                .AddLogger(new ConsoleLogger(false, new Dictionary<LogKind, ConsoleColor>
                {
                    { LogKind.Default, ConsoleColor.Gray },
                    { LogKind.Help, ConsoleColor.DarkGreen },
                    { LogKind.Header, ConsoleColor.Cyan },
                    { LogKind.Result, ConsoleColor.DarkCyan },
                    { LogKind.Statistic, ConsoleColor.Gray },
                    { LogKind.Info, ConsoleColor.DarkYellow },
                    { LogKind.Error, ConsoleColor.Red },
                    { LogKind.Warning, ConsoleColor.Yellow },
                    { LogKind.Hint, ConsoleColor.DarkCyan }
                }));

        switch (arg)
        {
            case "1":
                BenchmarkRunner.Run<DuplicatesBenchmarkExtended>(config);
                break;
            case "2":
                BenchmarkRunner.Run<QueryBenchmarkExtended>(config);
                break;
            case "3":
                BenchmarkRunner.Run<DuplicatesBenchmarkReduced>(config);
                break;
            case "4":
                BenchmarkRunner.Run<QueryBenchmarkReduced>(config);
                break;
            case "5":
                BenchmarkRunner.Run<MtQueryBenchmarkReduced>(config);
                break;
            case "6":
            default:
                BenchmarkRunner.Run<MtQueryBenchmarkExtended>(config);
                break;
        }

        // BenchmarkRunner.Run(
        // [
            // typeof(TokenizationBenchmark),
            // typeof(QueryBenchmarkGeneral),
            // typeof(LuceneBenchmark),
            // typeof(DuplicateBenchmarkGeneral),

            // typeof(DuplicatesBenchmarkExtended),
            // typeof(QueryBenchmarkExtended),
            // typeof(DuplicatesBenchmarkReduced),
            // typeof(QueryBenchmarkReduced),

            // typeof(MtQueryBenchmarkExtended),
            // typeof(MtQueryBenchmarkReduced),
            // typeof(StQueryBenchmarkExtended),
            // typeof(StQueryBenchmarkReduced)
        // ], config);
    }

    /// <summary>
    /// Запустить код в режиме, пригодном для профилирования.
    /// Запускается инициализация и запросы на RSSE токенайзере.
    /// </summary>
    private static async Task RunProfiling(bool runTokenizerBenchmarks = true)
    {
        // Шаг для инициализации.
        Console.WriteLine($"[{nameof(RunProfiling)}] starting..");

        IBenchmarkRunner benchmarkRunner = runTokenizerBenchmarks ?
            new QueryBenchmarkGeneral()
            //new DuplicateBenchmarkGeneral()
            //new QueryBenchmarkExtended { SearchType = new BenchmarkParameter<ExtendedSearchType>(SearchType: ExtendedSearchType.Legacy, Pool: true) }
            //new DuplicatesBenchmarkExtended() { SearchType = new BenchmarkParameter<ExtendedSearchType>(SearchType: ExtendedSearchType.Legacy, Pool: true) }
            //new QueryBenchmarkReduced { SearchType = new BenchmarkParameter<ReducedSearchType>(SearchType: ReducedSearchType.Legacy, Pool: true) }
            //new DuplicatesBenchmarkReduced() { SearchType = new BenchmarkParameter<ReducedSearchType>(SearchType: ReducedSearchType.Legacy, Pool: true) }
            //new MtQueryBenchmarkExtended { SearchType = new BenchmarkParameter<ExtendedSearchType>(SearchType: ExtendedSearchType.Legacy, Pool: true) }
            //new MtQueryBenchmarkReduced { SearchType = new BenchmarkParameter<ReducedSearchType>(SearchType: ReducedSearchType.Legacy, Pool: true) }
            //new StQueryBenchmarkExtended { SearchType = new BenchmarkParameter<ExtendedSearchType>(SearchType: ExtendedSearchType.Legacy, Pool: true) }
            //new StQueryBenchmarkReduced { SearchType = new BenchmarkParameter<ReducedSearchType>(SearchType: ReducedSearchType.Legacy, Pool: true) }
            : new LuceneBenchmark();

        var stopwatch = Stopwatch.StartNew();
        await benchmarkRunner.Initialize();
        stopwatch.Stop();

        var initializeMemory = GC.GetTotalAllocatedBytes();

        Console.WriteLine($"[{nameof(RunProfiling)}] | initialize | elapsed: {(double)stopwatch.ElapsedMilliseconds / 1000 / ProfilerIterations:F4} sec | " +
                          $"memory allocated: {initializeMemory / 1000000:N1} Mb.");

        // Шаг для прогрева.
        Console.WriteLine("---");
        Console.WriteLine("Runner is ready for warm-up. Press any key to continue.");
        Console.ReadKey(intercept: true);
        Console.WriteLine($"[{nameof(RunProfiling)}] | '{WarmUpIterations}' warm-up iterations starting..");

        for (var i = 0; i < WarmUpIterations; i++)
        {
            benchmarkRunner.RunBenchmark();
        }

        var warmupMemory = GC.GetTotalAllocatedBytes();

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);

        // Шаг для профилирования.
        Console.WriteLine("---");
        Console.WriteLine("Runner is ready for profiling. Press any key to continue.");
        Console.ReadKey(intercept: true);
        Console.WriteLine($"[{nameof(RunProfiling)}] | '{ProfilerIterations}' profiling iterations starting..");

        stopwatch.Restart();
        for (var i = 0; i < ProfilerIterations; i++)
        {
            benchmarkRunner.RunBenchmark();
        }
        stopwatch.Stop();

        var iterationsMemory = GC.GetTotalAllocatedBytes() - warmupMemory;

        Console.WriteLine($"[{nameof(RunProfiling)}] | elapsed total: '{(double)stopwatch.ElapsedMilliseconds / 1000:F4}' sec.");
        Console.WriteLine($"[{nameof(RunProfiling)}] | elapsed per request: '{(double)stopwatch.ElapsedMilliseconds / 1 / ProfilerIterations:F4}' mSec.");
        Console.WriteLine($"[{nameof(RunProfiling)}] | total memory allocated: '{iterationsMemory / 1000000:N1}' Mb.");
        Console.WriteLine($"[{nameof(RunProfiling)}] | memory allocated per request: '{iterationsMemory / 1000 / ProfilerIterations:N1}' Kb.");

        Console.WriteLine("---");
        Console.WriteLine("Execution completed, press any key to exit.");
        Console.ReadKey(intercept: true);
    }

    private static async Task RunValidation()
    {
        SearchResultValidation searchResultValidation = new();
        await searchResultValidation.TestSearchQuery();
        await searchResultValidation.TestDuplicates();
    }

    private static async Task RunTokenizationProfiling()
    {
        TokenizationBenchmark tokenizationBenchmark = new()
        {
            IndexType = IndexType.GeneralDirectLegacy
        };

        Console.WriteLine($"[{nameof(RunTokenizationProfiling)}] Starting..");

        await tokenizationBenchmark.SetupAsync();

        Console.WriteLine($"[{nameof(RunTokenizationProfiling)}] Started..");
        Console.WriteLine("---");

        Console.WriteLine("Runner is ready for profiling. Press any key to continue.");
        Console.ReadKey(intercept: true);

        var initialMemory = GC.GetTotalAllocatedBytes();
        var stopwatch = Stopwatch.StartNew();

        await tokenizationBenchmark.InitializeTokenizer();

        stopwatch.Stop();
        var tokenizationMemory = GC.GetTotalAllocatedBytes() - initialMemory;

        Console.WriteLine($"[{nameof(RunTokenizationProfiling)}] | Elapsed time total: '{(double)stopwatch.ElapsedMilliseconds / 1000:F4}' sec.");
        Console.WriteLine($"[{nameof(RunTokenizationProfiling)}] | Total memory allocated: '{tokenizationMemory / 1000000:N1}' Mb.");
        Console.WriteLine("---");

        Console.WriteLine("Execution completed, press any key to exit.");
        Console.ReadKey(intercept: true);
    }
}
