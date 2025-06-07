using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace SearchEngine.Benchmarks;

/// <summary>
/// Бенчмарки и профилирование.
/// </summary>
public static class DiagnosticsProgram
{
    internal const int ProfilerIterations = 50;
    private const int WarmUpIterations = 1;

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
            Environment.Exit(1);
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
        }
    }

    /// <summary>
    /// Запустить бенчмарки.
    /// </summary>
    private static void RunBenchmarks()
    {
        Console.WriteLine($"[{nameof(RunBenchmarks)}] starting..");

        // Запуск бенчмарков с возможностью дебага, либо без неё.
        // Job.ShortRun запустит бенчмарки в отдельных процессах.
        var config = Debugger.IsAttached
            ? new DebugInProcessConfig().WithOptions(ConfigOptions.DisableOptimizationsValidator)
            : DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .AddJob(Job.InProcess
                    .WithWarmupCount(1)
                    .WithLaunchCount(1)
                    .WithIterationCount(3))
                .AddDiagnoser(MemoryDiagnoser.Default);

        BenchmarkRunner.Run<TokenizerBenchmarks>(config);
    }

    /// <summary>
    /// Запустить код в режиме, пригодном для профилирования.
    /// </summary>
    private static async Task RunProfiling()
    {
        Console.WriteLine($"[{nameof(RunProfiling)}] starting..");

        var benchmark = new TokenizerBenchmarks();
        var initializationStopwatch = Stopwatch.StartNew();
        await TokenizerBenchmarks.InitializeEngineTokenizer();
        initializationStopwatch.Stop();
        var initializationMegabytes = GC.GetTotalAllocatedBytes() / 1000000;
        Console.WriteLine($"{nameof(TokenizerBenchmarks)} | initialization | " +
                          $"elapsed: {(double)initializationStopwatch.ElapsedMilliseconds / 1000 / ProfilerIterations:F4} sec | " +
                          $"memory allocated: {initializationMegabytes:N1} Mb.");

        // Чистим память перед запуском.
        // GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);

        Console.WriteLine("Runner is ready for warm-up. Press 'enter' to continue.");
        Console.ReadLine();
        Console.WriteLine("Warm-up starting..");

        for (var i = 0; i < WarmUpIterations; i++)
        {
            benchmark.BenchmarkEngineTokenizer();
            // tokenizerBenchmark.LuceneBenchmark();
        }
        var warmupMegabytes = GC.GetTotalAllocatedBytes();

        Console.WriteLine("Runner is ready for profiling. Press 'enter' to continue.");
        Console.ReadLine();
        Console.WriteLine($"'{ProfilerIterations}' iterations starting..");


        var iterationsStopwatch = Stopwatch.StartNew();

        for (var i = 0; i < ProfilerIterations; i++)
        {
            benchmark.BenchmarkEngineTokenizer();
            // tokenizerBenchmark.LuceneBenchmark();
        }

        iterationsStopwatch.Stop();
        var iterationsMegabytes = (GC.GetTotalAllocatedBytes() - warmupMegabytes) / 1000000;

        Console.WriteLine($"{nameof(TokenizerBenchmarks)} | iterations | " +
                          $"elapsed: {(double)iterationsStopwatch.ElapsedMilliseconds / 1000 / ProfilerIterations:F4} sec | " +
                          $"memory allocated: {iterationsMegabytes:N1} Mb.");

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}
