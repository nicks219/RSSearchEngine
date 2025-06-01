using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Contracts;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Tests.Integration.FakeDb.Api;
using SearchEngine.Tests.Integration.FakeDb.Extensions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace SearchEngine.Tests.Integration.FakeDb;

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
        var token = CancellationToken.None;

        // act:
        const string tagRequest = "new-1";
        using var _ = _factory!.CreateClient(_options!);
        using var serviceScope = _factory.Services.CreateScope();
        var mysqlRepo = (CatalogRepository<MysqlCatalogContext>)serviceScope.ServiceProvider.GetRequiredService(typeof(CatalogRepository<MysqlCatalogContext>));
        var npgsqlRepo = (CatalogRepository<NpgsqlCatalogContext>)serviceScope.ServiceProvider.GetRequiredService(typeof(CatalogRepository<NpgsqlCatalogContext>));
        mysqlRepo.EnsureNotNull();
        await mysqlRepo.CreateTagIfNotExists(tagRequest, token);
        var tagsFromMysql = await mysqlRepo.ReadTags(token);
        var tagsFromNpg = await npgsqlRepo.ReadTags(token);

        // assert:
        // теги сохраняются в верхнем регистре
        tagsFromMysql
            .Select(tagResult => tagResult.GetEnrichedName())
            .Should()
            .Contain(tagRequest.ToUpper());

        tagsFromNpg
            .Select(tagResult => tagResult.GetEnrichedName())
            .Should()
            .NotContain(tagRequest.ToUpper());
    }

    [TestMethod]
    public async Task ReaderAndWriterContexts_ShouldOperateIndependently()
    {
        var token = CancellationToken.None;

        // act:
        const string tag = "new-2";
        using var _ = _factory!.CreateClient(_options!);
        using var serviceScope = _factory.Services.CreateScope();
        var mysqlRepo = (CatalogRepository<MysqlCatalogContext>)serviceScope.ServiceProvider.GetRequiredService(typeof(CatalogRepository<MysqlCatalogContext>));
        var mysqlContext = (MysqlCatalogContext)serviceScope.ServiceProvider.GetRequiredService(typeof(MysqlCatalogContext));
        var npgsqlContext = (NpgsqlCatalogContext)serviceScope.ServiceProvider.GetRequiredService(typeof(NpgsqlCatalogContext));

        mysqlRepo.EnsureNotNull();
        await mysqlRepo.CreateTagIfNotExists(tag, token);
        var mysqlTags = mysqlContext
            .Tags
            .Select(x => x.Tag)
            .ToList();
        var npgsqlTags = npgsqlContext
            .Tags
            .Select(x => x.Tag)
            .ToList();

        // assert:
        mysqlTags
            .Should()
            .Contain(tag.ToUpper());

        npgsqlTags
            .Should()
            .NotContain(tag.ToUpper());
    }

    [TestMethod]
    public async Task IDataRepository_WritesToBothDatabases_WhenCreateTagCalled()
    {
        var token = CancellationToken.None;

        const string tag = "new-3";
        using var _ = _factory!.CreateClient(_options!);
        using var serviceScope = _factory.Services.CreateScope();
        var mirroredRepo = (IDataRepository)serviceScope.ServiceProvider.GetRequiredService(typeof(IDataRepository));
        var mysqlContext = (MysqlCatalogContext)serviceScope.ServiceProvider.GetRequiredService(typeof(MysqlCatalogContext));
        var npgsqlContext = (NpgsqlCatalogContext)serviceScope.ServiceProvider.GetRequiredService(typeof(NpgsqlCatalogContext));

        mirroredRepo.EnsureNotNull();

        // act:
        await mirroredRepo.CreateTagIfNotExists(tag, token);
        var mysqlTags = mysqlContext
            .Tags
            .Select(x => x.Tag)
            .ToList();
        var npgsqlTags = npgsqlContext
            .Tags
            .Select(x => x.Tag)
            .ToList();

        // assert:
        // теги сохраняются в верхнем регистре
        mysqlTags
            .Should()
            .Contain(tag.ToUpper());

        npgsqlTags
            .Should()
            .Contain(tag.ToUpper());
    }
}
