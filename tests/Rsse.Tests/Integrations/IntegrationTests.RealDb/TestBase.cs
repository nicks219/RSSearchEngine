using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations.IntegrationTests.RealDb;

public class TestBase : IDisposable
{
    protected static readonly SemaphoreSlim Semaphore = new(1, 1);
    protected static readonly CancellationToken Token = CancellationToken.None;
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static volatile bool _initialized;

    protected readonly IntegrationWebAppFactory<Startup> Factory = new ();
    protected readonly WebApplicationFactoryClientOptions Options = new()
    {
        BaseAddress = BaseAddress,
        HandleCookies = true
    };

    protected TestBase()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        if (_initialized) return;

        var ct = CancellationToken.None;
        var isGitHubAction = Docker.IsGitHubAction();
        if (isGitHubAction)
        {
            Console.WriteLine($"{nameof(IntegrationTests)} | dbs running in container(s)");
        }

        var sw = Stopwatch.StartNew();
        if (!isGitHubAction)
        {
            await Docker.CleanUpDbContainers(ct);
            await Docker.InitializeDbContainers(ct);
            Console.WriteLine($"docker warmup elapsed: {sw.Elapsed.TotalSeconds:0.000} sec");
        }

        using var client = Factory.CreateClient(Options);
        client.GetAsync(RouteConstants.SystemWaitWarmUpGetUrl, ct).GetAwaiter().GetResult();
        Console.WriteLine("host warmup completed");

        _initialized = true;
    }

    public void Dispose()
    {
        Factory.Dispose();
    }
}
