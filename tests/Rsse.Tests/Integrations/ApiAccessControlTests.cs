using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Common.Auth;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations;

/// <summary>
/// Тесты аутентификации и авторизации.
/// </summary>
[TestClass]
public class ApiAccessControlTests
{
    [ClassInitialize]
    public static void ApiAccessControlTestsSetup(TestContext context)
    {
        // arrange:
        _factory = new CustomWebAppFactory<AuthStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri,
            HandleCookies = true
        };
        // тест Api_DeleteNote_ByAuthorizedUser_ShouldSucceed удаляет данные, необходимые для Api_ReadTitleByNoteId_ReturnsTitle
    }

    [ClassCleanup]
    public static void CleanUp() => _factory!.Dispose();

    private static CustomWebAppFactory<AuthStartup>? _factory;
    private static WebApplicationFactoryClientOptions? _options;

    [TestMethod]
    public async Task Api_DeleteNote_ByUnauthenticatedUser_Returns401()
    {
        // act:
        using var client = _factory!.CreateClient(_options!);
        var uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);
        using var response = await client.DeleteAsync(uri);
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var statusCode = (int)response.StatusCode;

        // assert:
        statusCode.Should().Be(401);
        reason.Should().Be("Unauthorized");
        response.Should().NotBeNull();
        var shift = headers.FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);
        shift.Value.First().Should().Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task Api_DeleteNote_ByUnauthorizedUser_Returns403()
    {
        // act: invalid login:
        using var client = _factory!.CreateClient(_options!);
        var uri = new Uri("account/login?email=editor&password=editor", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();

        // act: request:
        uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
        response = await client.DeleteAsync(uri);
        var reason = response.ReasonPhrase;
        var content = await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;

        // assert:
        statusCode.Should().Be(403);
        reason.Should().Be("Forbidden");
        content.Should().NotBeNull();
        content.Should().Be("GET: access denied.");

        response.Dispose();
    }

    [TestMethod]
    public async Task Api_DeleteNote_ByAuthorizedUser_ShouldSucceed()
    {
        // act: valid login:
        using var client = _factory!.CreateClient(_options!);
        var uri = new Uri("account/login?email=admin&password=admin", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();

        // act: request:
        // запрос на удаление несуществующей заметки - чтобы не аффектить тесты, завязанные на её чтение
        uri = new Uri("api/catalog?id=2&pg=1", UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
        response = await client.DeleteAsync(uri);
        var reason = response.ReasonPhrase;
        var statusCode = (int)response.StatusCode;

        // assert:
        statusCode.Should().Be(200);
        reason.Should().Be("OK");

        response.Dispose();
    }
}
