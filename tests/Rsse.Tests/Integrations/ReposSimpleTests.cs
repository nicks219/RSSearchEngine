using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Context;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations;

[TestClass]
public class ReposSimpleTests
{
    [ClassInitialize]
    public static void ReposSimpleTestsSetup(TestContext _)
    {
        // arrange:
        _factory = new CustomWebAppFactory<SimpleMirrorStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };
    }

    [ClassCleanup]
    public static void CleanUp() => _factory.Dispose();

    private static CustomWebAppFactory<SimpleMirrorStartup> _factory;
    private static WebApplicationFactoryClientOptions _options;

    [TestMethod]
    public async Task MySqlRepoAndPostgresRepo_ShouldOperateIndependently()
    {
        // act:
        const string tag = "new-1";
        using var _ = _factory.CreateClient(_options);
        using var serviceScope = _factory.Services.CreateScope();
        await using var mysqlRepo = serviceScope.ServiceProvider.GetRequiredService<CatalogRepository<MysqlCatalogContext>>();
        await using var npgsqlRepo = serviceScope.ServiceProvider.GetRequiredService<CatalogRepository<NpgsqlCatalogContext>>();
        if (mysqlRepo == null || npgsqlRepo == null) throw new TestCanceledException("missing repo(s)");
        await mysqlRepo.CreateTagIfNotExists(tag);
        var tagsFromMysql = await mysqlRepo.ReadStructuredTagList();
        var tagsFromNpg = await npgsqlRepo.ReadStructuredTagList();

        // assert:
        // теги сохраняются в верхнем регистре
        tagsFromMysql.Should().Contain(tag.ToUpper());
        tagsFromNpg.Should().NotContain(tag.ToUpper());
    }

    [TestMethod]
    public async Task ReaderAndWriterContexts_ShouldOperateIndependently()
    {
        // act:
        const string tag = "new-2";
        using var _ = _factory.CreateClient(_options);
        using var serviceScope = _factory.Services.CreateScope();
        await using var repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
        await using var mysqlRepo = serviceScope.ServiceProvider.GetRequiredService<CatalogRepository<MysqlCatalogContext>>();
        if (mysqlRepo == null || repo == null) throw new TestCanceledException("missing repo(s)");
        await mysqlRepo.CreateTagIfNotExists(tag);
        var reader = repo.GetReaderContext()?.Tags?.Select(x => x.Tag).ToList();
        var writer = repo.GetPrimaryWriterContext()?.Tags?.Select(x => x.Tag).ToList();

        // assert:
        // теги сохраняются в верхнем регистре
        writer.Should().Contain(tag.ToUpper());
        reader.Should().NotContain(tag.ToUpper());
    }

    [TestMethod]
    public async Task IDataRepository_WritesToBothDatabases_WhenCreateTagCalled()
    {
        const string tag = "new-3";
        using var _ = _factory.CreateClient(_options);
        using var serviceScope = _factory.Services.CreateScope();
        await using var repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
        if (repo == null) throw new TestCanceledException("missing repo(s)");

        // act:
        await repo.CreateTagIfNotExists(tag);
        var reader = repo.GetReaderContext()?.Tags?.Select(x => x.Tag).ToList();
        var writer = repo.GetPrimaryWriterContext()?.Tags?.Select(x => x.Tag).ToList();

        // assert:
        // теги сохраняются в верхнем регистре
        writer.Should().Contain(tag.ToUpper());
        reader.Should().Contain(tag.ToUpper());
    }
}
