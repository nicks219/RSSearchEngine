using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Service.ApiModels;
using SearchEngine.Tests.Integration.RealDb.Extensions;
using static SearchEngine.Service.Configuration.RouteConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace SearchEngine.Tests.Integration.RealDb;

[TestClass]
public class IntegrationTests : TestBase
{
    [TestMethod]
    [DataRow($"{MigrationCopyGetUrl}")]
    [DataRow($"{MigrationCreateGetUrl}?databaseType=MySql")]
    [DataRow($"{MigrationCreateGetUrl}?databaseType=Postgres")]
    [DataRow($"{MigrationRestoreGetUrl}?databaseType=MySql")]
    [DataRow($"{MigrationRestoreGetUrl}?databaseType=Postgres")]
    [DataRow($"{MigrationCreateGetUrl}?fileName=123&databaseType=MySql")]
    public async Task Migration_Requests_ShouldApplyCorrectly(string uriString)
    {
        var emptyRequestContent = new StringContent(string.Empty);
        await ExecuteTest(uriString, HttpMethod.Get, emptyRequestContent, ResponseValidator);

        // clean up:
        await TestHelper.CleanUpDatabases(Factory, ct: Token);
        return;

        // assert:
        Task ResponseValidator(HttpResponseMessage message)
        {
            message.StatusCode.Should().Be(HttpStatusCode.OK);
            return Task.CompletedTask;
        }
    }

    private const string TextToFind = "раз два три четыре";
    private static int _processedId;

    public static IEnumerable<object?[]> AfterDatabaseCopyTestData =>
    [
        [$"{MigrationRestoreGetUrl}?databaseType=MySql", HttpMethod.Get,
            Request.Null,
            Response.SkipValidation()],

        [$"{MigrationCopyGetUrl}", HttpMethod.Get,
            Request.Null,
            Response.SkipValidation()],

        [$"{CreateNotePostUrl}", HttpMethod.Post,
            Request.CreateWithBracketsRequest,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                _processedId = response.EnsureNotNull().NoteIdExchange.EnsureNotNull().Value;
                response.Text.Should().Be("dump files created");
            })],

        [$"{UpdateNotePutUrl}", HttpMethod.Put,
            Request.UpdateWithoutIdRequests,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                _processedId = response.EnsureNotNull().NoteIdExchange.EnsureNotNull().Value;
                response.Text.Should().Be("раз два три четыре");
            })],

        [$"{ComplianceIndicesGetUrl}?text={TextToFind}", HttpMethod.Get,
            Request.Null,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<ComplianceResponse>();
                var complianceId = response.EnsureNotNull().Res!.Keys.ElementAt(0);
                _processedId.Should().Be(complianceId);
            })],

        [$"{ReadTagsForCreateAuthGetUrl}", HttpMethod.Get,
            Request.Null,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                var tags = response.EnsureNotNull().StructuredTags!;
                tags.Should().Contain("Авторские: 80");
                tags.Should().Contain("1");
            })]
    ];

    [TestMethod]
    [DynamicData(nameof(AfterDatabaseCopyTestData))]
    // мотивация теста: при неконсистентном состоянии ключей после миграции создание заметки упадёт на constraint
    public async Task Migration_AfterDatabaseCopy_PKSequencesRemainsValid(
        string uriString,
        HttpMethod requestMethod,
        NoteRequest? content,
        Func<HttpResponseMessage, Task> responseValidator)
    {
        if (content != null) content = content with { NoteIdExchange = _processedId };
        var requestContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        await ExecuteTest(uriString, requestMethod, requestContent, responseValidator);
    }

    public static IEnumerable<object?[]> AfterDatabaseRestoreTestData =>
    [
        [$"{CreateNotePostUrl}", HttpMethod.Post,
            Request.CreateWithBracketsRequest,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                var createdId = response.EnsureNotNull().NoteIdExchange;
                var textResponse = response.Text;
                createdId.Should().Be(1);
                textResponse.Should().BeEquivalentTo("dump files created");
            })],

        [$"{MigrationCreateGetUrl}?databaseType=Postgres", HttpMethod.Get,
            Request.Null,
            Response.Validate(_ => Task.CompletedTask)],

        [$"{MigrationRestoreGetUrl}?databaseType=Postgres", HttpMethod.Get,
            Request.Null,
            Response.Validate(_ => Task.CompletedTask)],

        [$"{CreateNotePostUrl}", HttpMethod.Post,
            Request.CreateRequest,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                var createdId = response.EnsureNotNull().NoteIdExchange;
                var textResponse = response.Text;
                createdId.Should().Be(2);
                textResponse.Should().BeEquivalentTo("dump files created");
            })]
    ];

    [TestMethod]
    [DynamicData(nameof(AfterDatabaseRestoreTestData))]
    // мотивация теста: при неконсистентном состоянии ключей после миграции создание заметки упадёт на constraint
    public async Task Migration_AfterDatabaseRestore_PKSequencesRemainsValid(
        string uriString,
        HttpMethod requestMethod,
        NoteRequest? content,
        Func<HttpResponseMessage, Task> responseValidator)
    {
        var requestContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        // Очищаем бд для первого тестового кейса.
        if (content?.Title == "[1]") await TestHelper.CleanUpDatabases(Factory, ct: Token);
        await ExecuteTest(uriString, requestMethod, requestContent, responseValidator);
    }

    private const string ReadNoteTestText = "рас дваа три";

    public static IEnumerable<object[]> ReadNoteTestData =>
    [
        [$"{MigrationRestoreGetUrl}?databaseType=MySql", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<StringResponse>();
                response.EnsureNotNull().Res.Should().Be("backup_9.dump");
            })],

        [$"{MigrationCopyGetUrl}", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<StringResponse>();
                response.EnsureNotNull().Res.Should().Be("success");
            })],

        [$"{CreateNotePostUrl}", HttpMethod.Post,
            Request.CreateContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[OK]");
                response.Text.Should().BeEquivalentTo("dump files created");
            })],

        [$"{CreateNotePostUrl}", HttpMethod.Post,
            Request.CreateContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[Already Exist]");
            })],

        [$"{ReadNotePostUrl}?id=946", HttpMethod.Post,
            Request.ReadContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[1]");
                response.EnsureNotNull().Text.Should().Be("посчитаем до четырёх");
            })],

        [$"{UpdateNotePutUrl}", HttpMethod.Put,
            Request.UpdateContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[1]");
                response.EnsureNotNull().Text.Should().Be("раз два три четыре");
            })],

        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(ReadNoteTestText)}", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<ComplianceResponse>();
                response.EnsureNotNull().Res.EnsureNotNull().Keys.First().Should().Be(946);
                response.EnsureNotNull().Res.EnsureNotNull().Values.First().Should().Be(0.5D);
            })],

        [$"{ReadNotePostUrl}?id=946", HttpMethod.Post,
            Request.ReadContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[1]");
                response.EnsureNotNull().Text.Should().Be("раз два три четыре");
            })]
    ];

    [TestMethod]
    [DynamicData(nameof(ReadNoteTestData))]
    // мотивация теста: рефакторинг Tuple<string, string> в ответе с заметкой
    public async Task ReadNoteTextAndTitle_Requests_ShouldCompleteSuccessful(
        string uriString,
        HttpMethod requestMethod,
        StringContent requestContent,
        Func<HttpResponseMessage, Task> responseValidator)
    {
        await ExecuteTest(uriString, requestMethod, requestContent, responseValidator);
    }

    private const string ReadCatalogPageTestText = "пасчитаим читырех";
    private const string TitleToFind = "Розенбаум Вечерняя Застольная Черт с ними за столом сидим поем пляшем";

    public static IEnumerable<object[]> ReadCatalogTestData =>
    [
        [$"{MigrationRestoreGetUrl}?databaseType=MySql", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<StringResponse>();
                response.EnsureNotNull().Res.Should().Be("backup_9.dump");
            })],

        [$"{MigrationCopyGetUrl}", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<StringResponse>();
                response.EnsureNotNull().Res.Should().Be("success");
            })],

        [$"{CreateNotePostUrl}", HttpMethod.Post,
            Request.CreateContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[OK]");
                response.Text.Should().Be("dump files created");
            })],

        [$"{CatalogPageGetUrl}?id=1", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<CatalogResponse>();
                response.EnsureNotNull().CatalogPage.EnsureNotNull().First().Title.Should()
                    .Be(" Ветлицкая Наталья - Непогода");
                response.CatalogPage.EnsureNotNull().First().NoteId.Should().Be(238);
                // 1я страница
                response.CatalogPage.EnsureNotNull().Count.Should().Be(10);
                response.PageNumber.Should().Be(1);
                response.NotesCount.Should().Be(930);
            })],

        // навигация "вперёд" по каталогу
        [$"{CatalogNavigatePostUrl}", HttpMethod.Post,
            Request.CatalogContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<CatalogResponse>();
                response.EnsureNotNull().CatalogPage.EnsureNotNull().First().Title.Should()
                    .Be("Агата Кристи - Вольно");
                response.CatalogPage.EnsureNotNull().First().NoteId.Should().Be(280);
                // 2я страница
                response.CatalogPage.EnsureNotNull().Count.Should().Be(10);
                response.PageNumber.Should().Be(2);
                response.NotesCount.Should().Be(930);
            })],

        // проверим наличие заметки 1
        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(TitleToFind)}", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<ComplianceResponse>();
                response.EnsureNotNull().Res.EnsureNotNull().Keys.First().Should().Be(1);
                var value = Math.Round(response.Res.EnsureNotNull().Values.First(), 2);
                value.Should().Be(1.2D);
            })],

        [$"{DeleteNoteUrl}?id=1&pg=2", HttpMethod.Delete,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<CatalogResponse>();
                response.EnsureNotNull().PageNumber.Should().Be(2);
                response.NotesCount.Should().Be(929);
            })],

        [$"{DeleteNoteUrl}?id=2&pg=1", HttpMethod.Delete,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<CatalogResponse>();
                response.EnsureNotNull().PageNumber.Should().Be(1);
                response.NotesCount.Should().Be(928);
            })],

        // проверим наличие заметки 946
        [$"{CatalogPageGetUrl}?id=1", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<CatalogResponse>();
                response.EnsureNotNull().CatalogPage.EnsureNotNull().First().Title.Should()
                    .Be(" Ветлицкая Наталья - Непогода");
                response.CatalogPage.EnsureNotNull().First().NoteId.Should().Be(238);
                // 1я страница
                response.CatalogPage.EnsureNotNull().Count.Should().Be(10);
                response.PageNumber.Should().Be(1);
                response.NotesCount.Should().Be(928);
            })],

        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(ReadCatalogPageTestText)}", HttpMethod.Get,
            Request.EmptyContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<ComplianceResponse>();
                response.EnsureNotNull().Res.EnsureNotNull().Keys.First().Should().Be(946);
                var value = Math.Round(response.Res.EnsureNotNull().Values.First(), 2);
                value.Should().Be(6.67D);
            })],

        [$"{ReadNotePostUrl}?id=946", HttpMethod.Post,
            Request.ReadContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be("[1]");
                response.Text.Should().Be("посчитаем до четырёх");
            })],

        // проверим отсутствие заметки 1
        [$"{ReadNotePostUrl}?id=1", HttpMethod.Post,
            Request.ReadContent,
            Response.Validate(async message =>
            {
                var response = await message.Content.ReadFromJsonAsync<NoteResponse>();
                response.EnsureNotNull().Title.Should().Be(string.Empty);
                response.Text.Should().Be(string.Empty);
            })],

        [$"{ComplianceIndicesGetUrl}?text={Uri.EscapeDataString(TitleToFind)}", HttpMethod.Get,
            Request.EmptyContent,
            Response.SkipValidation()]
    ];

    [TestMethod]
    [DynamicData(nameof(ReadCatalogTestData))]
    // мотивация теста: рефакторинг Tuple<string, int> в ответе каталога
    public async Task ReadCatalogPage_Requests_ShouldCompleteSuccessful(
        string uriString,
        HttpMethod requestMethod,
        StringContent requestContent,
        Func<HttpResponseMessage, Task> responseValidator)
    {
        await ExecuteTest(uriString, requestMethod, requestContent, responseValidator);
    }

    [TestMethod]
    public async Task UpdateCredos_Request_ShouldChangePassword()
    {
        // arrange:
        const string oldEmail = "1@2";
        const string oldPassword = "12";
        const string newEmail = "1@3";
        const string newPassword = "13";
        using var client = Factory.CreateClient(Options);
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

        using var request = new HttpRequestMessage(HttpMethod.Get, uriWithCorrectCredos);
        using var _ = await client.SendAsync(request, Token);
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

    /// <summary>
    /// Выполнить тест.
    /// </summary>
    /// <param name="uriString">Строка тестового запроса.</param>
    /// <param name="requestMethod">Метод тестового запроса.</param>
    /// <param name="requestContent">Контент тестового запроса.</param>
    /// <param name="responseValidator">Метод для проверки тестового запроса.</param>
    private async Task ExecuteTest(
        string uriString,
        HttpMethod requestMethod,
        StringContent requestContent,
        Func<HttpResponseMessage, Task> responseValidator)
    {
        // arrange:
        using var client = Factory.CreateClient(Options);
        var warmupResult = await client.GetAsync(SystemWaitWarmUpGetUrl);
        warmupResult.EnsureSuccessStatusCode();

        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var request = new HttpRequestMessage(requestMethod, uri);
        request.Content = requestContent;
        using var response = await client.SendAsync(request, Token);

        // assert:
        await responseValidator(response);

        // clean up:
        requestContent.Dispose();
    }
}
