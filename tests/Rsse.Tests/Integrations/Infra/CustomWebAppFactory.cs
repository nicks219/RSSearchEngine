using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using SearchEngine.Common.Auth;

namespace SearchEngine.Tests.Integrations.Infra;

internal class CustomWebAppFactory<T> : WebApplicationFactory<T> where T : class
{
    protected override IHostBuilder CreateHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                Environment.SetEnvironmentVariable(Constants.AspNetCoreEnvironmentName, Constants.TestingEnvironment);
                webBuilder.UseStartup<T>();
            });

        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        var host = base.CreateHost(builder);
        return host;
    }
}
