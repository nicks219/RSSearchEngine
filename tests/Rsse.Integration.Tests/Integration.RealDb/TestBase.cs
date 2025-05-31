using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tests.Integration.RealDb.Api;
using SearchEngine.Tests.Integration.RealDb.Infra;

namespace SearchEngine.Tests.Integration.RealDb;

/// <summary>
/// Базовый класс для тестов.
/// Обеспечивает однократный подъём докера на локальной разработке, "прогрев" и очистку хоста.
/// </summary>
public class TestBase : IDisposable
{
    protected static readonly CancellationToken Token = CancellationToken.None;
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static volatile bool _initialized;

    protected readonly IntegrationWebAppFactory<Startup> Factory = new();
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
        switch (isGitHubAction)
        {
            case true:
                Console.WriteLine($"{nameof(IntegrationTests)} | dbs running in container(s)");
                break;

            case false:
                {
                    var sw = Stopwatch.StartNew();
                    await Docker.CleanUpDbContainers(ct);
                    await Docker.InitializeDbContainers(ct);
                    Console.WriteLine($"docker warmup elapsed: {sw.Elapsed.TotalSeconds:0.000} sec");
                    break;
                }
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
