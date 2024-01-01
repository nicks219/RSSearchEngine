using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace SearchEngine.Tests.Integrations.Infra;

internal class CustomWebAppFactory : WebApplicationFactory<IntegrationStartup>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder()
            .UseEnvironment(Environments.Development)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<IntegrationStartup>());

        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        var host = base.CreateHost(builder);
        return host;
    }
}
