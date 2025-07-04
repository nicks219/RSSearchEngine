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
    /// Запускается инициализация и запросы на RSSE токенайзере.
    /// </summary>
    private static async Task RunProfiling(bool runRsse = true)
    {
        Console.WriteLine($"[{nameof(RunProfiling)}] starting..");

        var benchmark = new TokenizerBenchmarks();
        var stopwatch = Stopwatch.StartNew();

        switch (runRsse)
        {
            case true:
                await TokenizerBenchmarks.InitializeEngineTokenizer();
                break;
            default:
                await TokenizerBenchmarks.InitializeLucene();
                break;
        }

        stopwatch.Stop();
        var initializeMemory = GC.GetTotalAllocatedBytes();

        Console.WriteLine($"{nameof(TokenizerBenchmarks)} | initialize | elapsed: {(double)stopwatch.ElapsedMilliseconds / 1000 / ProfilerIterations:F4} sec | " +
                          $"memory allocated: {initializeMemory / 1000000:N1} Mb.");

        Console.WriteLine("Runner is ready for warm-up. Press 'enter' to continue.");
        Console.ReadLine();
        Console.WriteLine("Warm-up starting..");

        for (var i = 0; i < WarmUpIterations; i++)
        {
            switch (runRsse)
            {
                case true:
                    benchmark.BenchmarkEngineTokenizer();
                    break;
                default:
                    benchmark.BenchmarkLucene();
                    break;
            }
        }
        var warmupMemory = GC.GetTotalAllocatedBytes();

        Console.WriteLine("Runner is ready for profiling. Press 'enter' to continue.");
        Console.ReadLine();
        Console.WriteLine($"'{ProfilerIterations}' iterations starting..");

        stopwatch.Restart();
        for (var i = 0; i < ProfilerIterations; i++)
        {
            switch (runRsse)
            {
                case true:
                    benchmark.BenchmarkEngineTokenizer();
                    break;
                default:
                    benchmark.BenchmarkLucene();
                    break;
            }
        }
        stopwatch.Stop();
        var iterationsMemory = GC.GetTotalAllocatedBytes() - warmupMemory;

        Console.WriteLine($"{nameof(TokenizerBenchmarks)} | iterations | elapsed: {(double)stopwatch.ElapsedMilliseconds / 1000 / ProfilerIterations:F4} sec | " +
                          $"memory allocated: {iterationsMemory / 1000000:N1} Mb.");

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}
