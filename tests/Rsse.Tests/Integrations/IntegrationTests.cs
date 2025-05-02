using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Controllers;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Dto;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tooling.Contracts;

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
            BaseAddress = BaseAddress,
            HandleCookies = true
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


    // мотивация теста: при неконсистентном состоянии ключей после миграции создание заметки упадёт на constraint
    // отрефакторенный Integration_PKSequencesAreValid_AfterDatabaseCopy

    [TestMethod]
    [DataRow([
        "migration/restore?databaseType=MySql",
        "migration/copy",
        "api/create",
        "api/update"
    ])]
    // todo: тест и TestHelper разделить на части - практически "божественные объекты"
    public async Task Integration_PKSequencesAreValid_AfterDatabaseCopyWithViaAPI(string[] uris)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_options);
        await client.TryAuthorizeToService("1@2", "12");

        using var enumerator = TestHelper
            .GetEnumerableRequestContent(forUpdate: true)
            .GetEnumerator();
        enumerator.MoveNext();
        var processedId = 0;

        // act:
        foreach (var uri in uris)
        {
            if (uri is "api/create" or "api/update")
            {
                // POST
                var note = enumerator.Current;
                enumerator.MoveNext();
                if (uri == "api/update")
                {
                    // необходимо выставить id обновляемой заметки note.CommonNoteId
                    var dto = await note.ReadFromJsonAsync<NoteDto>();
                    dto.EnsureNotNull().CommonNoteId = processedId;
                    note = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                }

                using var postResponse = await client.PostAsync(uri, note);
                var deserializedPostResponse = await postResponse
                    .EnsureSuccessStatusCode()
                    .Content
                    .ReadFromJsonAsync<NoteDto>();
                processedId = deserializedPostResponse
                    .EnsureNotNull()
                    .CommonNoteId;// 946 - 946
                note.Dispose();
                continue;
            }

            // GET
            using var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
        }

        // текст из update
        const string textToFind = "раз два три четыре";
        var complianceId = await client.GetFirstComplianceIndexFromTokenizer(textToFind);
        var tags = await client.GetTagsFromReaderOnly();
        await client.DeleteNoteFromService(processedId);

        // assert:
        processedId
            .Should()
            .Be(complianceId);

        // тег из дампа
        tags.Should().Contain("Авторские: 80");

        // добавленный через create тег
        tags.Should().Contain("1");

        // clean up:
        TestHelper.CleanUpDatabases(factory);
    }

    // мотивация теста: при неконсистентном состоянии ключей после миграции создание заметки упадёт на constraint
    // отрефакторенный Integration_PKSequencesAreValid_AfterDatabaseRestore

    [TestMethod]
    [DataRow([
        "api/create",
        "migration/create?databaseType=Postgres",
        "migration/restore?databaseType=Postgres",
        "api/create"
        ])]
    public async Task Integration_PKSequencesAreValid_AfterDatabaseRestoreViaAPI(string[] uris)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_options);
        await client.TryAuthorizeToService("1@2", "12");

        using var enumerator = TestHelper
            .GetEnumerableRequestContent()
            .GetEnumerator();
        enumerator.MoveNext();
        var createdId = 0;
        var fakePathToDump = "";

        // act
        foreach (var uri in uris)
        {
            if (uri == "api/create")
            {
                // POST
                using var note = enumerator.Current;
                enumerator.MoveNext();
                using var postResponse = await client.PostAsync(uri, note);
                var deserializedPostResponse = await postResponse
                    .EnsureSuccessStatusCode()
                    .Content
                    .ReadFromJsonAsync<NoteDto>();
                createdId = deserializedPostResponse
                    .EnsureNotNull()
                    .CommonNoteId;// 1 - 2
                fakePathToDump = deserializedPostResponse.TextResponse;
                continue;
            }

            // GET
            using var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
        }

        // assert:
        // var separator = Path.DirectorySeparatorChar;
        createdId
            .Should()
            .BeGreaterThan(0);

        fakePathToDump
            .Should()
            .BeEquivalentTo("dump files created");
            // .BeEquivalentTo($"ClientApp/build{separator}dump.zip");

        // clean up:
        TestHelper.CleanUpDatabases(factory);
    }

    // WARN по результатам теста Integration_PKSequencesAreValid_AfterDatabaseCopyWithViaAPI:
    // I.      название заметки при создании тега можно очищать от скобочек
    // II.     надо текст внутренней ошибки возвращать "снизу" в ответе, в поле CommonErrorMessageResponse
    //          например исключение менеджера оверрайдится контроллером

    // WARN по результатам теста Integration_PKSequencesAreValid_AfterDatabaseRestoreViaAPI:
    // I.      на pg контексте: получил [CreateManager] CreateNote error: DETAIL: Key (TagId)=(1) is not present in table "Tag", тк заметка не создавалась, то и тег падал
    //         перенес создание тега до создания заметки - надо при удачном создании тега возвращаться, не пытаясь создать заметку
    // II.     надо текст внутренней ошибки возвращать "снизу" в ответе, в поле details
    // III.    на заметку не создаётся zip (только файлы) - а возвращается dump.zip
    // IV.     postgres: как ресториться из "последних" файлов? рестор сделан только из *.zip

    // удалить два теста:

    [TestMethod]
    [Ignore("отрефакторен")]
    [Obsolete("отрефакторен")]
    [ExcludeFromCodeCoverage]
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
    [Ignore("отрефакторен")]
    [Obsolete("отрефакторен")]
    [ExcludeFromCodeCoverage]
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
