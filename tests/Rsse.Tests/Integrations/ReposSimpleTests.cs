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
    [TestMethod]
    public async Task MySqlRepoAndPostgresRepo_ShouldOperateIndependently()
    {
        // arrange:
        var factory = new CustomWebAppFactory<SimpleMirrorStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };

        // act:
        const string tag = "new";
        using var _ = factory.CreateClient(options);
        await using var mysqlRepo = factory.HostInternal?.Services.GetRequiredService<CatalogRepository<MysqlCatalogContext>>();
        await using var npgsqlRepo = factory.HostInternal?.Services.GetRequiredService<CatalogRepository<NpgsqlCatalogContext>>();
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
        // arrange:
        var factory = new CustomWebAppFactory<SimpleMirrorStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };

        // act:
        const string tag = "new";
        using var _ = factory.CreateClient(options);
        await using var repo = factory.HostInternal?.Services.GetRequiredService<IDataRepository>();
        await using var mysqlRepo = factory.HostInternal?.Services.GetRequiredService<CatalogRepository<MysqlCatalogContext>>();
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
        // arrange:
        var factory = new CustomWebAppFactory<SimpleMirrorStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };
        const string tag = "new";
        using var _ = factory.CreateClient(options);
        await using var repo = factory.HostInternal?.Services.GetRequiredService<IDataRepository>();
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
