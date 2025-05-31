using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Tokenizer;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;
using Serilog.Core;

namespace SearchEngine.Tests.Integrations.IntegrationTests.RealDb;

public class TestBase
{
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    protected static readonly CancellationToken Token = CancellationToken.None;
    private static readonly Lock DockerInitialized = new();
    private static volatile bool initialized;
    protected IntegrationWebAppFactory<Startup> _factory;
    protected static readonly WebApplicationFactoryClientOptions _options = new()
    {
        BaseAddress = BaseAddress,
        HandleCookies = true
    };

    protected static TokenizerLock lok = new();
    protected static TokenizerLock lok2 = new();

    public TestBase()
    {
        // блокируемся до окончания инициализации конструктора.
        Init().GetAwaiter().GetResult();
    }

    //[ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public async Task Init(/*TestContext context*/)
    {
        using var _ = await lok.AcquireExclusiveLockAsync(CancellationToken.None);
        var ct = CancellationToken.None;

        //lock (DockerInitialized)
        {
            if (!initialized)
            {
                var isGitHubAction = Docker.IsGitHubAction();
                if (isGitHubAction)
                {
                    //context.WriteLine($"{nameof(IntegrationTests)} | dbs running in container(s)");
                }

                var sw = Stopwatch.StartNew();
                if (!isGitHubAction)
                {
                    await Docker.CleanUpDbContainers(ct);
                    await Docker.InitializeDbContainers(ct);
                }

                //context.WriteLine($"docker warmup elapsed: {sw.Elapsed.TotalSeconds:0.000} sec");
                await using var factory = new IntegrationWebAppFactory<Startup>();
                using var client = factory.CreateClient(_options);
                client.GetAsync(RouteConstants.SystemWaitWarmUpGetUrl, ct).GetAwaiter().GetResult();
                await TestHelper.CleanUpDatabases(factory, ct);
                //context.WriteLine("fixture created");
                Console.WriteLine("dockers created");
                initialized = true;
            }
        }
    }

    //[ClassCleanup(ClassCleanupBehavior.EndOfAssembly)]
    //public static async Task CleanUp() => await _factory.DisposeAsync();
}
