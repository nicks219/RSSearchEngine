using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Common;
using SearchEngine.Data.Dto;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations;

/// <summary>
/// Тесты аутентификации и авторизации.
/// </summary>
[TestClass]
public class ApiAccessControlTests
{
    private static CustomWebAppFactory<SqliteAccessControlStartup>? _factory;
    private static WebApplicationFactoryClientOptions? _options;

    [ClassInitialize]
    public static void ApiAccessControlTestsSetup(TestContext context)
    {
        _factory = new CustomWebAppFactory<SqliteAccessControlStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri,
            HandleCookies = true
        };
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void CleanUp() => _factory!.Dispose();

    [TestMethod]
    public async Task Api_Unauthorized_Delete_ShouldReturns401()
    {
        // arrange:
        var uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);
        using var client = _factory!.CreateClient(_options!);

        // act:
        using var response = await client.DeleteAsync(uri);
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.Unauthorized);
        reason.Should().Be(HttpStatusCode.Unauthorized.ToString());
        response.Should().NotBeNull();
        var shift = headers.FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);
        shift.Value.First().Should().Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task Api_Unauthenticated_Delete_ShouldReturns403()
    {
        // arrange:
        var uri = new Uri("account/login?email=editor&password=editor", UriKind.Relative);
        using var client = _factory!.CreateClient(_options!);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();
        uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);

        // act:
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
        response = await client.DeleteAsync(uri);
        var reason = response.ReasonPhrase;
        var content = await response.Content.ReadAsStringAsync();
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.Forbidden);
        reason.Should().Be(HttpStatusCode.Forbidden.ToString());
        content.Should().NotBeNull();
        content.Should().Be("GET: access denied.");

        response.Dispose();
    }

    [TestMethod]
    [DataRow("migration/copy")]
    [DataRow("migration/create?fileName=123&databaseType=MySql")]
    [DataRow("migration/restore?fileName=123&databaseType=MySql")]
    [DataRow("migration/download?filename=123")]
    [DataRow("account/check")]
    [DataRow("account/update?OldCredos.Email=1&OldCredos.Password=2&NewCredos.Email=3&NewCredos.Password=4")]
    [DataRow("api/create")]// GetStructuredTagListAsync
    [DataRow("api/update?id=1")]// GetInitialNote
    public async Task Api_Unauthorized_Get_ShouldReturns401(string uriString)
    {
        // arrange:
        using var client = _factory!.CreateClient(_options!);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.Unauthorized);
        reason.Should().Be(HttpStatusCode.Unauthorized.ToString());
        response.Should().NotBeNull();
        var shift = headers.FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);
        shift.Value.First().Should().Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    [DataRow("migration/upload")]
    [DataRow("api/create")]
    [DataRow("api/update")]
    public async Task Api_Unauthorized_Post_ShouldReturns401(string uriString)
    {
        // arrange:
        using var client = _factory!.CreateClient(_options!);
        var fileContent = new ByteArrayContent([0x1, 0x2, 0x3, 0x4]);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        var formData = new MultipartFormDataContent();
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.PostAsync(uri, formData);
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.Unauthorized);
        reason.Should().Be(HttpStatusCode.Unauthorized.ToString());
        response.Should().NotBeNull();
        var shift = headers.FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);
        shift.Value.First().Should().Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task Api_Authorized_Delete_ShouldReturns200()
    {
        // arrange:
        var uri = new Uri("account/login?email=admin&password=admin", UriKind.Relative);
        using var client = _factory!.CreateClient(_options!);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();
        // запрос на удаление несуществующей заметки - чтобы не аффектить тесты, завязанные на её чтение
        uri = new Uri("api/catalog?id=2&pg=1", UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });

        // act:
        response = await client.DeleteAsync(uri);
        var reason = response.ReasonPhrase;
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.OK);
        reason.Should().Be(HttpStatusCode.OK.ToString());

        response.Dispose();
    }

    [TestMethod]
    // следует запускать на mysql и postgres:
    // [DataRow("migration/copy")]
    // [DataRow("migration/create?fileName=123&databaseType=MySql")]
    // [DataRow("migration/restore?fileName=123&databaseType=MySql")]
    [DataRow("migration/download?filename=backup_9.dump")]
    [DataRow("account/check")]
    [DataRow("account/update?OldCredos.Email=admin&OldCredos.Password=admin&NewCredos.Email=admin&NewCredos.Password=admin")]
    [DataRow("api/create")]
    [DataRow("api/update?id=1")]
    public async Task Api_Authorized_Get_ShouldReturns200(string uriString)
    {
        // arrange:
        var uri = new Uri("account/login?email=admin&password=admin", UriKind.Relative);
        using var client = _factory!.CreateClient(_options!);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();
        uri = new Uri(uriString, UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });

        // act:
        response = await client.GetAsync(uri);
        var reason = response.ReasonPhrase;
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.OK);
        reason.Should().Be(HttpStatusCode.OK.ToString());

        response.Dispose();
    }

    [TestMethod]
    [DataRow("migration/upload", true)]// IFormFile
    [DataRow("api/create", false)]// json
    [DataRow("api/update", false)]// json
    public async Task Api_Authorized_Post_ShouldReturns200(string uriString, bool appendFile)
    {
        // arrange:
        var uri = new Uri("account/login?email=admin&password=admin", UriKind.Relative);
        using var client = _factory!.CreateClient(_options!);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();

        var fileContent = new ByteArrayContent([0x1, 0x2, 0x3, 0x4]);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        var json = new NoteDto();
        var jsonContent = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        dynamic content = appendFile ? new MultipartFormDataContent() : jsonContent;
        if (appendFile) content.Add(fileContent, "file", "file.txt");

        uri = new Uri(uriString, UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });

        // act:
        response = await client.PostAsync(uri, content);
        var reason = response.ReasonPhrase;
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(HttpStatusCode.OK);
        reason.Should().Be(HttpStatusCode.OK.ToString());

        response.Dispose();
    }
}
