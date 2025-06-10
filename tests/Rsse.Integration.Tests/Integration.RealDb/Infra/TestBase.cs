using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tests.Integration.RealDb.Api;

namespace SearchEngine.Tests.Integration.RealDb.Infra;

/// <summary>
/// Базовый класс для тестов.
/// Обеспечивает однократный подъём докера на локальной разработке, "прогрев" и очистку хоста.
/// </summary>
public class TestBase
{
    protected static readonly CancellationToken Token = CancellationToken.None;
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static volatile bool _initialized;

    protected static readonly WebApplicationFactoryClientOptions Options = new()
    {
        BaseAddress = BaseAddress,
        HandleCookies = true
    };

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task Initialize(TestContext testContext)
    {
        // Возможность быстро прервать подъём докера (может повлиять на его стейт).
        var token = testContext.CancellationTokenSource.Token;
        await InitializeAsync(token);
    }

    private static async Task InitializeAsync(CancellationToken ct)
    {
        if (_initialized) return;

        var isGitHubAction = Docker.IsGitHubAction();
        switch (isGitHubAction)
        {
            case true:
                Console.WriteLine($"{nameof(TestBase)} | dbs running in container(s)");
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

        await using var factory = new IntegrationWebAppFactory<Startup>();
        using var client = factory.CreateClient(Options);
        client.GetAsync(RouteConstants.SystemWaitWarmUpGetUrl, ct).GetAwaiter().GetResult();
        Console.WriteLine("host warmup completed");

        _initialized = true;
    }
}
