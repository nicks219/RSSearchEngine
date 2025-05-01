using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Controllers;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tools.MigrationAssistant;
using SearchEngine.Tests.Integrations.Extensions;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
namespace SearchEngine.Tests.Integrations;

[TestClass]
public class IntegrationTests
{
    private const string Tag = "new";
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static WebApplicationFactoryClientOptions _cookiesOptions;
    private static WebApplicationFactoryClientOptions _options;

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

        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = BaseAddress
        };

        _cookiesOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = BaseAddress, HandleCookies = true
        };
    }

    [TestMethod]
    [DataRow("migration/copy")]
    [DataRow("migration/create?databaseType=MySql")]
    [DataRow("migration/create?databaseType=Postgres")]
    [DataRow("migration/restore?databaseType=MySql")]
    [DataRow("migration/restore?databaseType=Postgres")]
    [DataRow("migration/create?fileName=123&databaseType=MySql")]
    public async Task Integration_Migrations_ShouldApplyCorrectly(string uriString)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_cookiesOptions);
        await client.TryAuthorizeToService("1@2", "12");
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var reason = response.ReasonPhrase;
        var statusCode = response.StatusCode;

        // asserts:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);

        reason
            .Should()
            .Be(HttpStatusCode.OK.ToString());

        // clean up:
        TestHelper.CleanUpDatabases(factory);
    }

    [TestMethod]
    // мотивация теста: при некорректном состоянии ключей после копирования создание заметки упадёт на constraint
    // todo: упрости тест, можно оставить в только вызов CreateNote после миграции и проверку отсутствия исключения
    public async Task Integration_PKSequencesAreValid_AfterDatabaseCopy()
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_options);
        var services = factory.HostInternal.EnsureNotNull().Services;
        await using var repo = services.GetRequiredService<IDataRepository>();
        var migrators = services.GetServices<IDbMigrator>().ToList();
        var mysqlMigrator = MigrationController.GetMigrator(migrators, DatabaseType.MySql);
        var tokenizer = services.GetRequiredService<ITokenizerService>();

        // NB: c pomelo иногда бывает исключение attempted to read past the end of the stream, разберись
        mysqlMigrator.Restore(string.Empty);
        await repo.CopyDbFromMysqlToNpgsql();

        using var scope = services.CreateScope();
        await using var scopedRepo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        const string text = "раз два три четыре";
        List<int> tags = [1, 2, 3];
        var note = TestHelper.GetNoteDto(tags);
        var noteForUpdate = TestHelper.GetNoteForUpdate(text);

        // act:
        await scopedRepo.CreateTagIfNotExists(Tag);
        var createdId = await scopedRepo.CreateNote(note);
        noteForUpdate.CommonNoteId = createdId;
        await scopedRepo.UpdateNote(tags, noteForUpdate);
        // repo не апдейтит кэш
        tokenizer.Initialize();

        using var response = await client.GetAsync($"api/compliance/indices?text={text}");
        var result = await response.Content.ReadAsStringAsync();
        var firstKey = JsonSerializer.Deserialize<ComplianceResponseModel>(result)?.res.Keys.ElementAt(0);
        Int64.TryParse(firstKey, out var complianceId);

        await scopedRepo.DeleteNote(createdId);
        var reader = repo.GetReaderContext()?.Tags?.Select(x => x.Tag).ToList();
        var writer = repo.GetPrimaryWriterContext()?.Tags?.Select(x => x.Tag).ToList();

        // assert:
        writer
            .Should()
            .Contain(Tag.ToUpper());

        reader
            .Should()
            .Contain(Tag.ToUpper());

        complianceId
            .Should()
            .Be(createdId);

        // clean up:
        TestHelper.CleanUpDatabases(factory);
    }

    [TestMethod]
    // мотивация теста: при некорректном состоянии ключей после миграции создание заметки упадёт на constraint
    // можно оставить в тесте только вызов CreateNote и проверку отсутствия исключения
    public async Task Integration_PKSequencesAreValid_AfterDatabaseRestore()
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var _ = factory.CreateClient(_options);
        var services = factory.HostInternal.EnsureNotNull().Services;
        await using var repo = services.GetRequiredService<IDataRepository>();
        var migrators = services.GetServices<IDbMigrator>().ToList();
        var pgsqlMigrator = MigrationController.GetMigrator(migrators, DatabaseType.Postgres);
        var note = TestHelper.GetNoteDto();

        // act:
        // тестовая база postgres не содержит данных (кроме users), следует добавить тег, чтобы сослаться на него в checkedTags
        await repo.CreateTagIfNotExists(Tag);
        pgsqlMigrator.Create(string.Empty);
        await repo.GetReaderContext().EnsureNotNull().Database.EnsureDeletedAsync();
        await repo.GetReaderContext().EnsureNotNull().Database.EnsureCreatedAsync();
        pgsqlMigrator.Restore(string.Empty);

        using var scope = services.CreateScope();
        await using var scopedRepo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var createdId = await scopedRepo.CreateNote(note);

        // assert:
        createdId
            .Should()
            .BeGreaterThan(0);

        // clean up:
        TestHelper.CleanUpDatabases(factory);
    }
}
