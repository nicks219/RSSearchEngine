using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Startup;
using SearchEngine.Domain.Contracts;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Extensions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace SearchEngine.Tests.Integrations;

[TestClass]
public class RepositoryTests
{
    private static readonly Uri BaseUri = new("http://localhost:5000/");
    private static CustomWebAppFactory<SqliteStartup>? _factory;
    private static WebApplicationFactoryClientOptions? _options;

    [ClassInitialize]
    public static async Task RepositoryTestsSetup(TestContext _)
    {
        // arrange:
        _factory = new CustomWebAppFactory<SqliteStartup>();
        _options = new WebApplicationFactoryClientOptions { BaseAddress = BaseUri };

        // NB: в тестах используется метод из Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
        // чтобы разрезолвить конфликт типов со сборкой Microsoft.Testing.Platform
        var configuration = (IConfiguration)_factory.Server.Services.GetRequiredService(typeof(IConfiguration));

        var defaultConnectionString = configuration[Startup.DefaultConnectionKey];
        var additionalConnectionString = configuration[Startup.AdditionalConnectionKey];

        // ждём коннекта до sqlite
        await using var defaultConnection = new SqliteConnection(defaultConnectionString);
        await using var additionalConnection = new SqliteConnection(additionalConnectionString);
        var count = 20;
        while (count-- > 0)
        {
            try
            {
                await defaultConnection.OpenAsync();
                await additionalConnection.OpenAsync();
                return;
            }
            catch (Exception)
            {
                Task.Delay(100).Wait();
            }
        }

        throw new TestCanceledException($"{nameof(RepositoryTests)} | SQLite connection(s) missing");
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void CleanUp() => _factory!.Dispose();

    [TestMethod]
    public async Task MySqlRepoAndPostgresRepo_ShouldOperateIndependently()
    {
        // act:
        const string tag = "new-1";
        using var _ = _factory!.CreateClient(_options!);
        using var serviceScope = _factory.Services.CreateScope();
        await using var mysqlRepo = (CatalogRepository<MysqlCatalogContext>)serviceScope.ServiceProvider.GetRequiredService(typeof(CatalogRepository<MysqlCatalogContext>));
        await using var npgsqlRepo = (CatalogRepository<NpgsqlCatalogContext>)serviceScope.ServiceProvider.GetRequiredService(typeof(CatalogRepository<NpgsqlCatalogContext>));
        mysqlRepo.EnsureNotNull();
        await mysqlRepo.CreateTagIfNotExists(tag);
        var tagsFromMysql = await mysqlRepo.ReadStructuredTagList();
        var tagsFromNpg = await npgsqlRepo.ReadStructuredTagList();

        // assert:
        // теги сохраняются в верхнем регистре
        tagsFromMysql
            .Should()
            .Contain(tag.ToUpper());

        tagsFromNpg
            .Should()
            .NotContain(tag.ToUpper());
    }

    [TestMethod]
    public async Task ReaderAndWriterContexts_ShouldOperateIndependently()
    {
        // act:
        const string tag = "new-2";
        using var _ = _factory!.CreateClient(_options!);
        using var serviceScope = _factory.Services.CreateScope();
        await using var repo = (IDataRepository)serviceScope.ServiceProvider.GetRequiredService(typeof(IDataRepository));
        await using var mysqlRepo = (CatalogRepository<MysqlCatalogContext>)serviceScope.ServiceProvider.GetRequiredService(typeof(CatalogRepository<MysqlCatalogContext>));
        mysqlRepo.EnsureNotNull();
        await mysqlRepo.CreateTagIfNotExists(tag);
        var reader = repo
            .GetReaderContext()?.Tags?
            .Select(x => x.Tag)
            .ToList();
        var writer = repo
            .GetPrimaryWriterContext()?.Tags?
            .Select(x => x.Tag)
            .ToList();

        // assert:
        writer
            .Should()
            .Contain(tag.ToUpper());

        reader
            .Should()
            .NotContain(tag.ToUpper());
    }

    [TestMethod]
    public async Task IDataRepository_WritesToBothDatabases_WhenCreateTagCalled()
    {
        const string tag = "new-3";
        using var _ = _factory!.CreateClient(_options!);
        using var serviceScope = _factory.Services.CreateScope();
        await using var repo = (IDataRepository)serviceScope.ServiceProvider.GetRequiredService(typeof(IDataRepository));
        repo.EnsureNotNull();

        // act:
        await repo.CreateTagIfNotExists(tag);
        var reader = repo
            .GetReaderContext()?.Tags?
            .Select(x => x.Tag)
            .ToList();
        var writer = repo
            .GetPrimaryWriterContext()?.Tags?
            .Select(x => x.Tag)
            .ToList();

        // assert:
        // теги сохраняются в верхнем регистре
        writer
            .Should()
            .Contain(tag.ToUpper());

        reader
            .Should()
            .Contain(tag.ToUpper());
    }
}
