using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Configuration;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Engine;
using SearchEngine.Infrastructure.Tokenizer;
using SearchEngine.Infrastructure.Tokenizer.Contracts;
using SearchEngine.Tests.Infrastructure.DAL;

namespace SearchEngine.Tests.Infrastructure;

public class TestServiceCollection<TScope> where TScope : class
{
    internal readonly IServiceScope Scope;
    internal readonly IServiceProvider Provider;

    public TestServiceCollection()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataRepository, TestDataRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddSingleton<ILogger<TScope>, TestLogger<TScope>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddTransient<ITokenizerService, TokenizerService>();

        var serviceProvider = services.BuildServiceProvider();
        Provider = serviceProvider;
        Scope = serviceProvider.CreateScope();
    }
}
