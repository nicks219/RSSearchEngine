using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Controllers;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tools.MigrationAssistant;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
namespace SearchEngine.Tests.Integrations;

[TestClass]
public class IntegrationTests
{
    [ClassInitialize]
    public static void IntegrationTestsSetup(TestContext context)
    {
        var isGitHubAction = Docker.IsGitHubAction();
        if (isGitHubAction)
        {
            context.WriteLine($"{nameof(IntegrationTests)} | dbs running in container(s)");
        }

        // arrange:
        var sw = Stopwatch.StartNew();
        if (!isGitHubAction)
        {
            Docker.CleanUpDbContainers();
            Docker.InitializeDbContainers();
        }

        context.WriteLine($"docker warmup elapsed: {sw.Elapsed.TotalSeconds:0.000} sec");
    }

    [TestMethod]
    public async Task Integration_PKSequencesAreValid_AfterDatabaseCopy()
    {
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions { BaseAddress = baseUri };

        using var client = factory.CreateClient(options);
        await using var repo = factory.HostInternal?.Services.GetRequiredService<IDataRepository>();
        if (repo == null) throw new TestCanceledException("missing repo(s)");
        var migrators = factory.HostInternal?.Services.GetServices<IDbMigrator>();
        if (migrators == null) throw new TestCanceledException("missing migrators");
        var dbMigrators = migrators.ToList();
        var mysqlMigrator = MigrationController.GetMigrator(dbMigrators, DatabaseType.MySql);
        if (mysqlMigrator == null) throw new TestCanceledException("missing migrator(s)");
        var tokenizer = factory.HostInternal?.Services.GetRequiredService<ITokenizerService>();
        if (tokenizer == null) throw new TestCanceledException("missing tokenizer");

        // NB: рестору требуется файл миграции на пути ClientApp\build\backup_9.dump
        // NB: редко Attempted to read past the end of the stream, разберись
        mysqlMigrator.Restore(string.Empty);
        await repo.CopyDbFromMysqlToNpgsql();

        using var scope = factory.Server.Services.CreateScope();
        await using var scopedRepo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        const string tag = "new";
        const string text = "раз два три четыре";
        List<int> tags = [1, 2, 3];
        List<int> tagsForUpdate = [4];
        var note = new NoteDto { TitleRequest = "название", TextRequest = "раз два три", TagsCheckedRequest = tags};
        var noteForUpdate = new NoteDto { TitleRequest = "название", TextRequest = text, TagsCheckedRequest = tagsForUpdate};

        // act:
        await scopedRepo.CreateTagIfNotExists(tag);
        var createdId = await scopedRepo.CreateNote(note);
        noteForUpdate.CommonNoteId = createdId;
        await scopedRepo.UpdateNote(tags, noteForUpdate);
        // repo не апдейтит кэш
        tokenizer.Initialize();

        using var response = await client.GetAsync($"api/compliance/indices?text={text}");
        var result = await response.Content.ReadAsStringAsync();
        var firstKey = JsonSerializer.Deserialize<ResponseModel>(result)?.res.Keys.ElementAt(0);
        Int64.TryParse(firstKey, out var complianceId);

        await scopedRepo.DeleteNote(createdId);

        // assert:
        var reader = repo.GetReaderContext()?.Tags?.Select(x => x.Tag).ToList();
        var writer = repo.GetPrimaryWriterContext()?.Tags?.Select(x => x.Tag).ToList();
        writer.Should().Contain(tag.ToUpper());
        reader.Should().Contain(tag.ToUpper());

        complianceId.Should().Be(createdId);
    }

    [TestMethod]
    public async Task Integration_PKSequencesAreValid_AfterDatabaseRestore()
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions { BaseAddress = baseUri };

        using var _ = factory.CreateClient(options);
        await using var repo = factory.HostInternal?.Services.GetRequiredService<IDataRepository>();
        if (repo == null) throw new TestCanceledException("missing repo(s)");
        var migrators = factory.HostInternal?.Services.GetServices<IDbMigrator>();
        if (migrators == null) throw new TestCanceledException("missing migrators");
        var dbMigrators = migrators.ToList();
        var pgsqlMigrator = MigrationController.GetMigrator(dbMigrators, DatabaseType.Postgres);
        if (pgsqlMigrator == null) throw new TestCanceledException("missing migrator(s)");
        List<int> tags = [1, 2, 3];
        var note = new NoteDto { TitleRequest = "тестовая запись", TextRequest = "раз два три", TagsCheckedRequest = tags};

        // act:
        pgsqlMigrator.Create(string.Empty);
        await repo.GetReaderContext()?.Database.EnsureDeletedAsync()!;
        await repo.GetReaderContext()?.Database.EnsureCreatedAsync()!;
        pgsqlMigrator.Restore(string.Empty);

        using var scope = factory.Server.Services.CreateScope();
        await using var scopedRepo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var createdId = await scopedRepo.CreateNote(note);

        // assert:
        createdId.Should().BeGreaterThan(0);
    }

    public class ResponseModel
    {
        public required Dictionary<string, double> res { get; set; }
    }
}
