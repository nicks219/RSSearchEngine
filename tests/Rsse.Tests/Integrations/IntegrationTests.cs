using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tests.Units.Dto;

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
                    var dto = await note.ReadFromJsonAsync<NoteRequest>();
                    dto = dto.EnsureNotNull() with { NoteIdExchange = processedId };
                    note = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                }

                using var postResponse = await client.PostAsync(uri, note);
                var deserializedPostResponse = await postResponse
                    .EnsureSuccessStatusCode()
                    .Content
                    .ReadFromJsonAsync<NoteResponse>();
                processedId = deserializedPostResponse
                    .EnsureNotNull()
                    .NoteIdExchange;// 946 - 946
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
                    .ReadFromJsonAsync<NoteResponse>();
                createdId = deserializedPostResponse
                    .EnsureNotNull()
                    .NoteIdExchange;// 1 - 2
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

    private const string ReadNoteTestText = "рас дваа три";
    public static IEnumerable<object[]> ReadNoteTestData =>
    [
        ["migration/restore?databaseType=MySql", Request.Get, "{\"res\":\"backup_9.dump\"}", "", TestHelper.Empty],
        ["migration/copy", Request.Get, "success", "", TestHelper.Empty],

        ["api/create", Request.Post, "[OK]", "dump files created", TestHelper.CreateContent],
        ["api/create", Request.Post, "[Already Exist]", "", TestHelper.CreateContent],
        ["api/read?id=946", Request.Post, "[1]", "посчитаем до четырёх", TestHelper.ReadContent],
        ["api/update", Request.Post, "[1]", "раз два три четыре", TestHelper.UpdateContent],

        [$"api/compliance/indices?text={Uri.EscapeDataString(ReadNoteTestText)}", Request.Get, "946", "0.5", TestHelper.Empty],
        ["api/read?id=946", Request.Post, "[1]", "раз два три четыре", TestHelper.ReadContent]
    ];

    [TestMethod]
    [DynamicData(nameof(ReadNoteTestData))]
    // мотивация теста: рефакторинг Tuple<string, string> в ответе с заметкой

    public async Task Api_ReadNoteTextAndTitle_ShouldCompleteSuccessful(string uriString, Request type, string? titleExpected, string? textExpected, StringContent content)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_options);
        await client.TryAuthorizeToService("1@2", "12");
        var uri = new Uri(uriString, UriKind.Relative);

        switch (type)
        {
            // act:
            case Request.Get:
            {
                using var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                if (uriString.StartsWith("migration"))
                {
                    // assert:
                    var res = await response.Content.ReadAsStringAsync();
                    res.Should().Be(titleExpected);
                    break;
                }

                var result = await response.Content.ReadFromJsonAsync<ComplianceResponseModel>();

                // assert:
                result.EnsureNotNull().Res.Keys.First().Should().Be(titleExpected);
                _ = double.TryParse(textExpected, out var doubleExpected);
                result.EnsureNotNull().Res.Values.First().Should().Be(doubleExpected);
                break;
            }
            case Request.Post:
            {
                using var response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<NoteResponse>();

                // assert:
                result.EnsureNotNull().TitleResponse.Should().Be(titleExpected);
                result.EnsureNotNull().TextResponse.Should().Be(textExpected);
                break;
            }

            default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        content.Dispose();
    }

    // todo: переименуй магические числа
    private const string ReadCatalogPageTestText = "пасчитаим читырех";
    private const string TitleToFind = "Розенбаум Вечерняя Застольная Черт с ними за столом сидим поем пляшем";
    public static IEnumerable<object[]> ReadCatalogTestData =>
    [
        ["migration/restore?databaseType=MySql", Request.Get, typeof(OkObjectResult), "{\"res\":\"backup_9.dump\"}", "", TestHelper.Empty],
        ["migration/copy", Request.Get, typeof(OkObjectResult), "success", "", TestHelper.Empty],

        ["api/create", Request.Post, typeof(NoteResponse), "[OK]", "dump files created", TestHelper.CreateContent],
        ["api/catalog?id=1", Request.Get, typeof(CatalogResponse), "1", "930", TestHelper.Empty],
        ["api/catalog", Request.Post, typeof(CatalogResponse), "2", "930", TestHelper.CatalogContent],// навигация >>>
        // // проверим наличие заметки 1
        [$"api/compliance/indices?text={Uri.EscapeDataString(TitleToFind)}", Request.Get, typeof(ComplianceResponseModel), "1", "1.2", TestHelper.Empty],
        ["api/catalog?id=1&pg=2", Request.Delete, typeof(CatalogResponse), "2", "929", TestHelper.Empty],
        ["api/catalog?id=2&pg=1", Request.Delete, typeof(CatalogResponse), "1", "928", TestHelper.Empty],
        // проверим наличие заметки 946
        ["api/catalog?id=1", Request.Get, typeof(CatalogResponse), "1", "928", TestHelper.Empty],
        [$"api/compliance/indices?text={Uri.EscapeDataString(ReadCatalogPageTestText)}", Request.Get, typeof(ComplianceResponseModel), "946", "6.67", TestHelper.Empty],
        ["api/read?id=946", Request.Post, typeof(NoteResponse), "[1]", "посчитаем до четырёх", TestHelper.ReadContent],
        // проверим отсутствие заметки 1
        ["api/read?id=1", Request.Post, typeof(NoteResponse), "", "", TestHelper.ReadContent],
        [$"api/compliance/indices?text={Uri.EscapeDataString(TitleToFind)}", Request.Get, typeof(ComplianceResponseModel), "{}", "", TestHelper.Empty]
    ];

    [TestMethod]
    [DynamicData(nameof(ReadCatalogTestData))]
    // todo: ComplianceResponseModel используется только для тестов, но не в самом коде - примени
    // мотивация теста: рефакторинг Tuple<string, int> в ответе со страницей каталога

    public async Task Api_ReadCatalogPage_ShouldCompleteSuccessful(
        string uriString,
        Request type, Type responseType, string firstExpected, string secondExpected, StringContent content)
    {
        // arrange:
        // todo: тест новый хост каждый раз поднимает - перенеси в IntegrationTestsSetup
        await using var factory = new CustomWebAppFactory<IntegrationStartup>();
        using var client = factory.CreateClient(_options);
        await client.TryAuthorizeToService("1@2", "12");

        var uri = new Uri(uriString, UriKind.Relative);
        switch (type)
        {
            // act:
            case Request.Get:
            {
                using var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                if (responseType == typeof(ComplianceResponseModel))
                {
                    var asString = (await response.Content.ReadFromJsonAsync(typeof(object))).EnsureNotNull().ToString();
                    if (asString.EnsureNotNull() == firstExpected)
                    {
                        break;
                    }

                    var deserialized = JsonSerializer.Deserialize<SearchEngine.Tests.Integrations.Dto.ComplianceResponseModel>(asString);
                    // assert:
                    deserialized.EnsureNotNull().res.Keys.First().Should().Be(firstExpected);
                    var value = Math.Round(deserialized.EnsureNotNull().res.Values.First(), 2);
                    value.Should().Be(double.Parse(secondExpected));
                    break;
                }

                if (responseType == typeof(OkObjectResult))
                {
                    // assert: для миграций
                    var res = await response.Content.ReadAsStringAsync();
                    res.Should().Be(firstExpected);
                    break;
                }

                var result = await response.Content.ReadFromJsonAsync(responseType);
                if (responseType == typeof(CatalogResponse))
                {
                    // assert: CatalogPage: List<Tuple<string, int>>()
                    (result as CatalogResponse).EnsureNotNull().CatalogPage.EnsureNotNull().Count.Should().Be(10); // 1я страница
                    (result as CatalogResponse).EnsureNotNull().PageNumber.Should().Be(int.Parse(firstExpected));
                    (result as CatalogResponse).EnsureNotNull().NotesCount.Should().Be(int.Parse(secondExpected));
                }

                break;
            }
            case Request.Post:
            {
                using var response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync(responseType);
                if (responseType == typeof(CatalogResponse))
                {
                    // assert: CatalogPage: List<Tuple<string, int>>()
                    (result as CatalogResponse).EnsureNotNull().CatalogPage.EnsureNotNull().Count.Should().Be(10);// 2я страница
                    (result as CatalogResponse).EnsureNotNull().PageNumber.Should().Be(int.Parse(firstExpected));
                    (result as CatalogResponse).EnsureNotNull().NotesCount.Should().Be(int.Parse(secondExpected));
                }
                if (responseType == typeof(NoteResponse))
                {
                    // assert:
                    (result as NoteResponse).EnsureNotNull().TitleResponse.Should().Be(firstExpected);
                    (result as NoteResponse).EnsureNotNull().TextResponse.Should().Be(secondExpected);
                }

                break;
            }
            case Request.Delete:
            {
                using var response = await client.DeleteAsync(uri);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<CatalogResponse>();

                // assert:
                result.EnsureNotNull().PageNumber.Should().Be(int.Parse(firstExpected));
                result.EnsureNotNull().NotesCount.Should().Be(int.Parse(secondExpected));
                break;
            }

            default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        content.Dispose();
    }
}
