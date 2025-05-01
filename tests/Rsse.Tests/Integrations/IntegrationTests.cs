using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Npgsql;
using SearchEngine.Controllers;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tools.MigrationAssistant;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
namespace SearchEngine.Tests.Integrations;

[TestClass]
public class IntegrationTests
{
    private const string Tag = "new";
    private static WebApplicationFactoryClientOptions _cookiesOptions;
    private static WebApplicationFactoryClientOptions _options;

    [ClassInitialize]
    public static void IntegrationTestsSetup(TestContext context)
    {
        _cookiesOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost:5000/"), HandleCookies = true
        };
        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

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
    [DataRow("migration/copy")]
    [DataRow("migration/create?databaseType=MySql")]
    [DataRow("migration/create?databaseType=Postgres")]
    [DataRow("migration/restore?databaseType=MySql")]
    [DataRow("migration/restore?databaseType=Postgres")]
    [DataRow("migration/create?fileName=123&databaseType=MySql")]
    public async Task Integration_Migrations_ShouldApplyCorrectly(string uriString)
    {
        // arrange:
        var uri = new Uri("account/login?email=1@2&password=12", UriKind.Relative);
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_cookiesOptions);
        using var authResponse = await client.GetAsync(uri);
        authResponse.EnsureSuccessStatusCode();

        var headers = authResponse.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
        uri = new Uri(uriString, UriKind.Relative);

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

        // чистим таблицы
        CleanUpDatabases(factory);
    }

    [TestMethod]
    public async Task Integration_PKSequencesAreValid_AfterDatabaseCopy()
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_options);
        await using var repo = factory.HostInternal?.Services.GetRequiredService<IDataRepository>();

        var migrators = factory.HostInternal?.Services.GetServices<IDbMigrator>();
        if (migrators == null) throw new TestCanceledException("missing migrators");
        var dbMigrators = migrators.ToList();
        var mysqlMigrator = MigrationController.GetMigrator(dbMigrators, DatabaseType.MySql);
        var tokenizer = factory.HostInternal?.Services.GetRequiredService<ITokenizerService>();

        repo.ThrowIfNull();
        mysqlMigrator.ThrowIfNull();
        tokenizer.ThrowIfNull();

        // NB: рестору требуется файл миграции на пути ClientApp\build\backup_9.dump
        // NB: редко Attempted to read past the end of the stream, разберись
        mysqlMigrator.Restore(string.Empty);
        await repo.CopyDbFromMysqlToNpgsql();

        using var scope = factory.Server.Services.CreateScope();
        await using var scopedRepo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        const string text = "раз два три четыре";
        List<int> tags = [1, 2, 3];
        List<int> tagsForUpdate = [4];
        var note = new NoteDto { TitleRequest = "название", TextRequest = "раз два три", TagsCheckedRequest = tags};
        var noteForUpdate = new NoteDto { TitleRequest = "название", TextRequest = text, TagsCheckedRequest = tagsForUpdate};

        // act:
        await scopedRepo.CreateTagIfNotExists(Tag);
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

        // чистим таблицы
        CleanUpDatabases(factory);
    }

    [TestMethod]
    public async Task Integration_PKSequencesAreValid_AfterDatabaseRestore()
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var _ = factory.CreateClient(_options);
        await using var repo = factory.HostInternal?.Services.GetRequiredService<IDataRepository>();
        var migrators = factory.HostInternal?.Services.GetServices<IDbMigrator>().ToList();
        migrators.ThrowIfNull();
        var pgsqlMigrator = MigrationController.GetMigrator(migrators, DatabaseType.Postgres);
        repo.ThrowIfNull();
        pgsqlMigrator.ThrowIfNull();
        List<int> checkedTags = [1];
        var note = new NoteDto { TitleRequest = "тестовая запись", TextRequest = "раз два три", TagsCheckedRequest = checkedTags};

        // act:
        // тк тестовая база postgres не содержит данных (кроме users), следует добавить тег, чтобы сослаться на него в checkedTags
        await repo.CreateTagIfNotExists(Tag);
        pgsqlMigrator.Create(string.Empty);
        await repo.GetReaderContext()?.Database.EnsureDeletedAsync()!;
        await repo.GetReaderContext()?.Database.EnsureCreatedAsync()!;
        pgsqlMigrator.Restore(string.Empty);

        using var scope = factory.Server.Services.CreateScope();
        await using var scopedRepo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var createdId = await scopedRepo.CreateNote(note);

        // assert:
        createdId
            .Should()
            .BeGreaterThan(0);

        // чистим таблицы
        CleanUpDatabases(factory);
    }

    /// <summary>
    /// Очистка таблиц двух баз данных
    /// </summary>
    /// <param name="factory"></param>
    private static void CleanUpDatabases(CustomWebAppFactory<IntegrationStartup> factory)
    {
        var pgConnectionString = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString(Startup.AdditionalConnectionKey);
        var mysqlConnectionString = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString(Startup.DefaultConnectionKey);

        using var pgConnection = new NpgsqlConnection(pgConnectionString);
        pgConnection.Open();
        var commands = new List<string>
        {
            // """TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "TagsToNotes" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Tag" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Note" RESTART IDENTITY CASCADE;"""
        };
        foreach (var command in commands)
        {
            using var cmd = new NpgsqlCommand(command, pgConnection);
            cmd.ExecuteNonQuery();
        }
        pgConnection.Close();

        using var mysqlConnection = new MySqlConnection(mysqlConnectionString);
        mysqlConnection.Open();
        commands =
        [
            "SET FOREIGN_KEY_CHECKS = 0;",
            // "TRUNCATE TABLE `Users`;",
            "TRUNCATE TABLE `Tag`;",
            "TRUNCATE TABLE `Note`;",
            "TRUNCATE TABLE `TagsToNotes`;",
            "SET FOREIGN_KEY_CHECKS = 1;",
        ];
        foreach (var command in commands)
        {
            using var cmd = new MySqlCommand(command, mysqlConnection);
            cmd.ExecuteNonQuery();
        }
        mysqlConnection.Close();
    }

    public class ResponseModel
    {
        // ReSharper disable once CollectionNeverUpdated.Global
        // ReSharper disable once InconsistentNaming
        public required Dictionary<string, double> res { get; init; }
    }
}
