using System;
using System.Diagnostics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchEngine.Benchmarks;

[TestClass]
public sealed class RsseBenchmarkRunner
{
    [TestMethod]
    public void RunBenchmarks()
    {
        var isGitHubCi = Environment.GetEnvironmentVariable("DOTNET_CI");
        if (isGitHubCi == "true")
        {
            Console.WriteLine($"[{nameof(RsseBenchmarkRunner)}] skipped on CI");
            return;
        }

        // Запуск бенчмарков с возможностью дебага, либо без неё.
        var config = Debugger.IsAttached
            ? new DebugInProcessConfig().WithOptions(ConfigOptions.DisableOptimizationsValidator)
            // можно указать null чтобы BenchmarkDotNet сам выбрал дефолтный конфиг.
            : DefaultConfig.Instance;

        BenchmarkRunner.Run<TokenizerBenchmark>(config);
    }
}
