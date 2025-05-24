using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Service.ApiModels;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;
using static SearchEngine.Service.Configuration.RouteConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
namespace SearchEngine.Tests.Integrations;

[TestClass]
public class IntegrationTests
{
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static readonly CancellationToken Token = CancellationToken.None;

    private static CustomWebAppFactory<IntegrationStartup> _factory;
    private static WebApplicationFactoryClientOptions _cookiesOptions;
    private static WebApplicationFactoryClientOptions _options;

    [ClassInitialize]
    public static async Task IntegrationTestsSetup(TestContext context)
    {
        var ct = context.CancellationTokenSource.Token;
        var isGitHubAction = Docker.IsGitHubAction();
        if (isGitHubAction)
        {
            context.WriteLine($"{nameof(IntegrationTests)} | dbs running in container(s)");
        }

        var sw = Stopwatch.StartNew();
        if (!isGitHubAction)
        {
            await Docker.CleanUpDbContainers(ct);
            await Docker.InitializeDbContainers(ct);
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

        _factory = new CustomWebAppFactory<IntegrationStartup>();
    }

    [TestInitialize]
    public void IntegrationTestsSetup() => _factory = new CustomWebAppFactory<IntegrationStartup>();

    [TestCleanup]
    public async Task IntegrationTestsCleanup() => await _factory.DisposeAsync();

    [TestMethod]
    [DataRow($"{MigrationCopyGetUrl}")]
    [DataRow($"{MigrationCreateGetUrl}?databaseType=MySql")]
    [DataRow($"{MigrationCreateGetUrl}?databaseType=Postgres")]
    [DataRow($"{MigrationRestoreGetUrl}?databaseType=MySql")]
    [DataRow($"{MigrationRestoreGetUrl}?databaseType=Postgres")]
    [DataRow($"{MigrationCreateGetUrl}?fileName=123&databaseType=MySql")]
    public async Task Integration_Migrations_ShouldApplyCorrectly(string uriString)
    {
        // arrange:
        using var client = _factory.CreateClient(_cookiesOptions);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.SendTestRequest(Request.Get, uri, ct: Token);
        var reason = response.ReasonPhrase;
        var statusCode = response.StatusCode;

        // asserts:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.OK));

        // clean up:
        await TestHelper.CleanUpDatabases(_factory, ct: Token);
    }

    public static IEnumerable<object?[]> AfterCopyTestData =>
    [
        [$"{MigrationRestoreGetUrl}?databaseType=MySql", Request.Get, typeof(StringResponse), null, null, null],
        [$"{MigrationCopyGetUrl}", Request.Get, typeof(StringResponse), null, null, null],
        [$"{CreateNotePostUrl}", Request.Post, typeof(NoteResponse), null, "dump files created", new NoteRequest { Title = "[1]", Text = "посчитаем до четырёх", CheckedTags = [1] }],
        [$"{UpdateNotePutUrl}", Request.Put, typeof(NoteResponse), null, "раз два три четыре", new NoteRequest { Title = "[1]", Text = "раз два три четыре", CheckedTags = [1] }],

        [$"{ComplianceIndicesGetUrl}?text={TextToFind}", Request.Get, typeof(ComplianceResponse), null, null, null],
        [$"{ReadTagsForCreateAuthGetUrl}", Request.Get, typeof(NoteResponse), "Авторские: 80", "1", null],
    ];
    private const string TextToFind = "раз два три четыре";
    private static int _processedId;

    [TestMethod]
    [DynamicData(nameof(AfterCopyTestData))]
    // мотивация теста: при неконсистентном состоянии ключей после миграции создание заметки упадёт на constraint
    public async Task Integration_PKSequencesAreValid_AfterDatabaseCopyWithViaAPI(
        string uri, Request requestMethod, Type responseType, string? firstExpected, string? secondExpected, NoteRequest? content)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        if (content != null) content = content with { NoteIdExchange = _processedId };
        using var jsonContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        // act:
        using var response = await client.SendTestRequest(requestMethod, new Uri(uri, UriKind.Relative), jsonContent, ct: Token);
        var result = await response.Content.ReadFromJsonAsync(responseType);
        var asNote = CastTo<NoteResponse>(result.EnsureNotNull());
        var asCompliance = CastTo<ComplianceResponse>(result.EnsureNotNull());

        // assert:
        switch (requestMethod)
        {
            case Request.Post:
            case Request.Put:
                {
                    _processedId = asNote.EnsureNotNull().NoteIdExchange.EnsureNotNull().Value; // 946 - 946

                    asNote.Text
                        .Should()
                        .Be(secondExpected);
                    break;
                }
            case Request.Get:
                if (uri.StartsWith(ComplianceIndicesGetUrl))
                {
                    var complianceId = asCompliance.EnsureNotNull().Res!.Keys.ElementAt(0);

                    _processedId
                        .Should()
                        .Be(complianceId);
                }

                if (uri.StartsWith(ReadTagsForCreateAuthGetUrl))
                {
                    var tags = asNote.EnsureNotNull().StructuredTags!;
                    // тег из дампа
                    tags
                        .Should()
                        .Contain(firstExpected);
                    // добавленный через create тег
                    tags
                        .Should()
                        .Contain(secondExpected);
                }

                break;
            case Request.Delete:
            default:
                throw new ArgumentOutOfRangeException(nameof(requestMethod), requestMethod, null);
        }
    }

    public static IEnumerable<object?[]> AfterRestoreTestData =>
    [
        [$"{CreateNotePostUrl}", Request.Post, 1, "dump files created", new NoteRequest { Title = "[1]", Text = "посчитаем до четырёх", CheckedTags = [1] }],
        [$"{MigrationCreateGetUrl}?databaseType=Postgres", Request.Get, null, null, null],
        [$"{MigrationRestoreGetUrl}?databaseType=Postgres", Request.Get, null, null, null],
        [$"{CreateNotePostUrl}", Request.Post, 2, "dump files created", new NoteRequest { Title = "1", Text = "посчитаем до четырёх", CheckedTags = [1] }]
    ];

    [TestMethod]
    [DynamicData(nameof(AfterRestoreTestData))]
    // мотивация теста: при неконсистентном состоянии ключей после миграции создание заметки упадёт на constraint
    public async Task Integration_PKSequencesAreValid_AfterDatabaseRestoreViaAPI(string uri, Request requestMethod,
        int? idExpected, string? textExpected, NoteRequest? content)
    {
        //очищаем базу на старте:
        if (idExpected == 1) await TestHelper.CleanUpDatabases(_factory, ct: Token);

        // arrange:
        using var client = _factory.CreateClient(_options);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        using var jsonContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        // act:
        using var response = await client.SendTestRequest(requestMethod, new Uri(uri, UriKind.Relative), jsonContent, ct: Token);

        switch (requestMethod)
        {
            case Request.Post:
                {
                    var deserializedResponse = await response
                        .Content
                        .ReadFromJsonAsync<NoteResponse>();
                    var createdId = deserializedResponse
                        .EnsureNotNull()
                        .NoteIdExchange; // 1 - 2
                    var textResponse = deserializedResponse.Text;

                    // assert:
                    createdId
                        .Should()
                        .Be(idExpected);

                    textResponse
                        .Should()
                        .BeEquivalentTo(textExpected);
                    break;
                }
            case Request.Get:
                {
                    response.EnsureSuccessStatusCode();
                    break;
                }
            case Request.Delete:
            case Request.Put:
            default:
                throw new ArgumentOutOfRangeException(nameof(requestMethod), requestMethod, null);
        }
    }

    private const string ReadNoteTestText = "рас дваа три";
    public static IEnumerable<object[]> ReadNoteTestData =>
    [
        [$"{MigrationRestoreGetUrl}?databaseType=MySql", Request.Get, typeof(StringResponse), "backup_9.dump", "", TestHelper.Empty],
        [$"{MigrationCopyGetUrl}", Request.Get, typeof(StringResponse), "success", "", TestHelper.Empty],

        [$"{CreateNotePostUrl}", Request.Post, typeof(NoteResponse), "[OK]", "dump files created", TestHelper.CreateContent],
        [$"{CreateNotePostUrl}", Request.Post, typeof(NoteResponse), "[Already Exist]", "", TestHelper.CreateContent],
        [$"{ReadNotePostUrl}?id=946", Request.Post, typeof(NoteResponse), "[1]", "посчитаем до четырёх", TestHelper.ReadContent],
        [$"{UpdateNotePutUrl}", Request.Put, typeof(NoteResponse), "[1]", "раз два три четыре", TestHelper.UpdateContent],

        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(ReadNoteTestText)}", Request.Get, typeof(ComplianceResponse), "946", "0.5", TestHelper.Empty],
        [$"{ReadNotePostUrl}?id=946", Request.Post, typeof(NoteResponse), "[1]", "раз два три четыре", TestHelper.ReadContent]
    ];

    [TestMethod]
    [DynamicData(nameof(ReadNoteTestData))]
    // мотивация теста: рефакторинг Tuple<string, string> в ответе с заметкой
    public async Task Api_ReadNoteTextAndTitleSequence_ShouldCompleteSuccessful(string uriString, Request requestMethod, Type responseType,
        string? firstExpected, string? secondExpected, StringContent requestContent)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);
        // act:
        using var response = await client.SendTestRequest(requestMethod, uri, requestContent, ct: Token);
        var result = (await response.Content.ReadFromJsonAsync(responseType)).EnsureNotNull();
        var asCompliance = CastTo<ComplianceResponse>(result);
        var asMigration = CastTo<StringResponse>(result);
        var asNote = CastTo<NoteResponse>(result);

        // assert:
        switch (requestMethod)
        {
            case Request.Get:
                {
                    if (responseType == typeof(StringResponse))
                    {
                        asMigration.EnsureNotNull().Res.Should().Be(firstExpected);
                        break;
                    }

                    _ = int.TryParse(firstExpected, out var firstInt);
                    asCompliance.EnsureNotNull().Res.EnsureNotNull().Keys.First().Should().Be(firstInt);
                    _ = double.TryParse(secondExpected, out var doubleExpected);
                    asCompliance.EnsureNotNull().Res.EnsureNotNull().Values.First().Should().Be(doubleExpected);
                    break;
                }
            case Request.Post:
            case Request.Put:
                {
                    asNote.EnsureNotNull().Title.Should().Be(firstExpected);
                    asNote.EnsureNotNull().Text.Should().Be(secondExpected);
                    break;
                }

            case Request.Delete:
            default: throw new ArgumentOutOfRangeException(nameof(requestMethod), requestMethod, null);
        }

        // clean up:
        requestContent.Dispose();
    }

    // todo: дать имя магическим числам
    private const string ReadCatalogPageTestText = "пасчитаим читырех";
    private const string TitleToFind = "Розенбаум Вечерняя Застольная Черт с ними за столом сидим поем пляшем";
    public static IEnumerable<object[]> ReadCatalogTestData =>
    [
        [$"{MigrationRestoreGetUrl}?databaseType=MySql", Request.Get, typeof(StringResponse), "backup_9.dump", "", TestHelper.Empty],
        [$"{MigrationCopyGetUrl}", Request.Get, typeof(StringResponse), "success", "", TestHelper.Empty],

        [$"{CreateNotePostUrl}", Request.Post, typeof(NoteResponse), "[OK]", "dump files created", TestHelper.CreateContent],
        [$"{CatalogPageGetUrl}?id=1", Request.Get, typeof(CatalogResponse), "1", "930", TestHelper.Empty],
        // навигация "вперёд" по каталогу
        [$"{CatalogNavigatePostUrl}", Request.Post, typeof(CatalogResponse), "2", "930", TestHelper.CatalogContent],
        // проверим наличие заметки 1
        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(TitleToFind)}", Request.Get, typeof(ComplianceResponse), "1", "1.2", TestHelper.Empty],
        [$"{DeleteNoteUrl}?id=1&pg=2", Request.Delete, typeof(CatalogResponse), "2", "929", TestHelper.Empty],
        [$"{DeleteNoteUrl}?id=2&pg=1", Request.Delete, typeof(CatalogResponse), "1", "928", TestHelper.Empty],
        // проверим наличие заметки 946
        [$"{CatalogPageGetUrl}?id=1", Request.Get, typeof(CatalogResponse), "1", "928", TestHelper.Empty],
        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(ReadCatalogPageTestText)}", Request.Get, typeof(ComplianceResponse), "946", "6.67", TestHelper.Empty],
        [$"{ReadNotePostUrl}?id=946", Request.Post, typeof(NoteResponse), "[1]", "посчитаем до четырёх", TestHelper.ReadContent],
        // проверим отсутствие заметки 1
        [$"{ReadNotePostUrl}?id=1", Request.Post, typeof(NoteResponse), "", "", TestHelper.ReadContent],
        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(TitleToFind)}", Request.Get, typeof(ComplianceResponse), "{}", "", TestHelper.Empty]
    ];

    [TestMethod]
    [DynamicData(nameof(ReadCatalogTestData))]
    // мотивация теста: рефакторинг Tuple<string, int> в ответе каталога
    // todo: задействовать в контроллерах модельки для тестов - ComplianceResponseModel, MigrationResponseModel
    public async Task Api_ReadCatalogPageSequence_ShouldCompleteSuccessful(
        string uriString, Request requestMethod, Type responseType, string firstExpected, string secondExpected,
        StringContent requestContent)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);
        // act:
        using var response = await client.SendTestRequest(requestMethod, uri, requestContent, ct: Token);
        var result = (await response.Content.ReadFromJsonAsync(responseType)).EnsureNotNull();

        var asCompliance = CastTo<ComplianceResponse>(result);
        var asMigration = CastTo<StringResponse>(result);
        var asCatalog = CastTo<CatalogResponse>(result);
        var asNote = CastTo<NoteResponse>(result);

        // assert:
        switch (requestMethod)
        {
            case Request.Get:
                {
                    if (responseType == typeof(ComplianceResponse))
                    {
                        if (asCompliance.EnsureNotNull().Res == null && firstExpected == "{}")
                        {
                            break;
                        }

                        _ = int.TryParse(firstExpected, out var asInt);
                        asCompliance.Res.EnsureNotNull().Keys.First().Should().Be(asInt);
                        var value = Math.Round(asCompliance.Res.EnsureNotNull().Values.First(), 2);
                        value.Should().Be(double.Parse(secondExpected));
                        break;
                    }

                    if (responseType == typeof(StringResponse))
                    {
                        asMigration.EnsureNotNull().Res.Should().Be(firstExpected);
                        break;
                    }

                    if (responseType == typeof(CatalogResponse))
                    {
                        asCatalog.EnsureNotNull().CatalogPage.EnsureNotNull().First().Title.Should()
                            .Be(" Ветлицкая Наталья - Непогода");
                        asCatalog.CatalogPage.EnsureNotNull().First().NoteId.Should().Be(238);
                        // 1я страница
                        asCatalog.CatalogPage.EnsureNotNull().Count.Should().Be(10);
                        asCatalog.PageNumber.Should().Be(int.Parse(firstExpected));
                        asCatalog.NotesCount.Should().Be(int.Parse(secondExpected));
                    }

                    break;
                }
            case Request.Post:
                {
                    if (responseType == typeof(CatalogResponse))
                    {
                        asCatalog.EnsureNotNull().CatalogPage.EnsureNotNull().First().Title.Should()
                            .Be("Агата Кристи - Вольно");
                        asCatalog.CatalogPage.EnsureNotNull().First().NoteId.Should().Be(280);
                        // 2я страница
                        asCatalog.CatalogPage.EnsureNotNull().Count.Should().Be(10);
                        asCatalog.PageNumber.Should().Be(int.Parse(firstExpected));
                        asCatalog.NotesCount.Should().Be(int.Parse(secondExpected));
                    }

                    if (responseType == typeof(NoteResponse))
                    {
                        asNote.EnsureNotNull().Title.Should().Be(firstExpected);
                        asNote.Text.Should().Be(secondExpected);
                    }

                    break;
                }
            case Request.Delete:
                {
                    asCatalog.EnsureNotNull().PageNumber.Should().Be(int.Parse(firstExpected));
                    asCatalog.NotesCount.Should().Be(int.Parse(secondExpected));
                    break;
                }

            case Request.Put:
            default: throw new ArgumentOutOfRangeException(nameof(requestMethod), requestMethod, null);
        }

        // clean up:
        requestContent.Dispose();
    }

    [TestMethod]
    public async Task Integration_UpdateCredosCorrectly_ShouldChangePassword()
    {
        // arrange:
        const string oldEmail = "1@2";
        const string oldPassword = "12";
        const string newEmail = "1@3";
        const string newPassword = "13";
        using var client = _factory.CreateClient(_cookiesOptions);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService(oldEmail, oldPassword, ct: Token);

        // act:
        var queryParams = new Dictionary<string, string?>
        {
            ["OldCredos.Email"] = oldEmail,
            ["OldCredos.Password"] = oldPassword,
            ["NewCredos.Email"] = newEmail,
            ["NewCredos.Password"] = newPassword,
        };
        var queryStringWithCorrectCredos = QueryHelpers.AddQueryString(AccountUpdateGetUrl, queryParams);
        var uriWithCorrectCredos = new Uri(queryStringWithCorrectCredos, UriKind.Relative);

        // act:
        using var _ = await client.SendTestRequest(Request.Get, uriWithCorrectCredos, ct: Token);
        Exception? exceptionOnIncorrectAuthorization = null;
        try
        {
            // incorrect credos:
            await client.TryAuthorizeToService(oldEmail, oldPassword, ct: Token);
        }
        catch (Exception exception)
        {
            exceptionOnIncorrectAuthorization = exception;
        }

        // asserts:
        await client.TryAuthorizeToService(newEmail, newPassword, ct: Token);
        exceptionOnIncorrectAuthorization.Should().NotBeNull();
    }

    // выполнить каст
    private static T? CastTo<T>(object value) where T : class => value as T;
}
