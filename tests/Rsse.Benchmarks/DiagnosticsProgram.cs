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
using RsseEngine.Benchmarks.Performance;
using static RsseEngine.Benchmarks.Constants;

namespace RsseEngine.Benchmarks;

/// <summary>
/// Бенчмарки и профилирование.
/// </summary>
public static class DiagnosticsProgram
{
    /// <summary>
    /// Выбор между режимами измерения и профилирования производительности.
    /// </summary>
    /// <param name="args">Выбор режима: "bech"/"profile".</param>
    public static async Task Main(string[] args)
    {
        var isGitHubCi = Environment.GetEnvironmentVariable("DOTNET_CI");
        if (isGitHubCi == "true")
        {
            Console.WriteLine($"[{nameof(DiagnosticsProgram)}] benchmark runner skipped on CI");
            Environment.Exit(1);
        }

        if (args.Length != 1)
        {
            Console.WriteLine($"[{nameof(DiagnosticsProgram)}] invalid args, usage: <bench>|<profile>");
            Console.WriteLine("Run benchmarks...");
            RunBenchmarks();
            Environment.Exit(0);
        }

        var arg = args[0];
        switch (arg)
        {
            case "bench":
                RunBenchmarks();
                break;

            case "profile":
                await RunProfiling();
                break;

            default:
                RunBenchmarks();
                break;
        }
    }

    /// <summary>
    /// Запустить бенчмарки.
    /// </summary>
    private static void RunBenchmarks()
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
                    .WithIterationCount(10))
                .AddDiagnoser(MemoryDiagnoser.Default)
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

        BenchmarkRunner.Run([
            /*typeof(QueryBenchmark),
            typeof(LuceneBenchmark),
            typeof(DuplicatesBenchmark),*/
            typeof(DuplicatesBenchmarkExtended),
            typeof(DuplicatesBenchmarkReduced),
            typeof(QueryBenchmarkExtended),
            typeof(QueryBenchmarkReduced),
        ], config);
    }

    /// <summary>
    /// Запустить код в режиме, пригодном для профилирования.
    /// Запускается инициализация и запросы на RSSE токенайзере.
    /// </summary>
    private static async Task RunProfiling(bool runTokenizerBenchmarks = true)
    {
        // Шаг для инициализации.
        Console.WriteLine($"[{nameof(RunProfiling)}] starting..");

        IBenchmarkRunner benchmarkRunner = runTokenizerBenchmarks
            ? new QueryBenchmarkGeneral()
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
        Console.WriteLine($"[{nameof(RunProfiling)}] | elapsed per request: '{(double)stopwatch.ElapsedMilliseconds / 1000 / ProfilerIterations:F4}' sec.");
        Console.WriteLine($"[{nameof(RunProfiling)}] | total memory allocated: '{iterationsMemory / 1000000:N1}' Mb.");
        Console.WriteLine($"[{nameof(RunProfiling)}] | memory allocated per request: '{iterationsMemory / 1000 / ProfilerIterations:N1}' Kb.");

        Console.WriteLine("---");
        Console.WriteLine("Execution completed, press any key to exit.");
        Console.ReadKey(intercept: true);
    }
}
