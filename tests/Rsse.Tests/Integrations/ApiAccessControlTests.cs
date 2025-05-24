using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Extensions;
using static SearchEngine.Service.Configuration.RouteConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace SearchEngine.Tests.Integrations;

/// <summary>
/// Тесты аутентификации и авторизации.
/// </summary>
[TestClass]
public class ApiAccessControlTests
{
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static readonly CancellationToken Token = CancellationToken.None;

    private static CustomWebAppFactory<Startup> _factory;
    private static WebApplicationFactoryClientOptions _options;

    [ClassInitialize]
    public static void ApiAccessControlTestsSetup(TestContext context)
    {
        _factory = new CustomWebAppFactory<Startup>();
        var baseUri = BaseAddress;
        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri,
            HandleCookies = true
        };
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void CleanUp() => _factory.Dispose();

    [TestMethod]
    public async Task Api_Unauthorized_Delete_ShouldReturns401()
    {
        // arrange:
        var uri = new Uri($"{DeleteNoteUrl}?id=1&pg=1", UriKind.Relative);
        using var client = _factory.CreateClient(_options);

        // act:
        using var response = await client.DeleteAsync(uri);
        var statusCode = response.StatusCode;
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var shift = headers
            .FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.Unauthorized));

        shift.Value.First()
            .Should()
            .Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task Api_Unauthenticated_Delete_ShouldReturns403()
    {
        // arrange:
        const string unauthenticated = "editor";
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(unauthenticated, unauthenticated, ct: Token);
        var uri = new Uri($"{DeleteNoteUrl}?id=1&pg=1", UriKind.Relative);

        // act:
        using var response = await client.DeleteAsync(uri);
        var statusCode = response.StatusCode;
        var reason = response.ReasonPhrase;
        var content = await response
            .Content
            .ReadAsStringAsync();

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.Forbidden));

        content
            .Should()
            .Be("GET: access denied.");
    }

    [TestMethod]
    [DataRow($"{MigrationCopyGetUrl}")]
    [DataRow($"{MigrationCreateGetUrl}?fileName=123&databaseType=MySql")]
    [DataRow($"{MigrationRestoreGetUrl}?fileName=123&databaseType=MySql")]
    [DataRow($"{MigrationDownloadGetUrl}?filename=123")]
    [DataRow($"{AccountCheckGetUrl}")]
    [DataRow($"{AccountUpdateGetUrl}?OldCredos.Email=1&OldCredos.Password=2&NewCredos.Email=3&NewCredos.Password=4")]
    [DataRow($"{ReadTagsForCreateAuthGetUrl}")]
    [DataRow($"{ReadNoteWithTagsForUpdateAuthGetUrl}?id=1")]
    public async Task Api_Unauthorized_Get_ShouldReturns401(string uriString)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var statusCode = response.StatusCode;
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var shift = headers
            .FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.Unauthorized));

        shift.Value.First()
            .Should()
            .Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    [DataRow($"{MigrationUploadPostUrl}", Request.Post)]
    [DataRow($"{CreateNotePostUrl}", Request.Post)]
    [DataRow($"{UpdateNotePutUrl}", Request.Put)]
    public async Task Api_Unauthorized_PostAndPut_ShouldReturns401(string uriString, Request requestMethod)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        using MultipartFormDataContent content = TestHelper.GetRequestContent(true);
        var uri = new Uri(uriString, UriKind.Relative);
        var ct = CancellationToken.None;

        // act:
        using var response = await client.SendTestRequest(requestMethod, uri, content, verify: false, ct: ct);
        var statusCode = response.StatusCode;
        var reason = response.ReasonPhrase;
        var headers = response.Headers;
        var shift = headers
            .FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.Unauthorized));

        shift.Value.First()
            .Should()
            .Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task Api_Authorized_Delete_ShouldReturns200()
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        // запрос на удаление несуществующей заметки, чтобы не аффектить тесты, завязанные на её чтение
        var uri = new Uri($"{DeleteNoteUrl}?id=2&pg=1", UriKind.Relative);

        // act:
        using var response = await client.DeleteAsync(uri);
        var statusCode = response.StatusCode;
        var reason = response.ReasonPhrase;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.OK));
    }

    [TestMethod]
    [DataRow($"{MigrationDownloadGetUrl}?filename=backup_9.dump")]
    [DataRow($"{AccountCheckGetUrl}")]
    [DataRow($"{AccountUpdateGetUrl}?OldCredos.Email=admin&OldCredos.Password=admin&NewCredos.Email=admin&NewCredos.Password=admin")]
    [DataRow($"{ReadTagsForCreateAuthGetUrl}")]
    [DataRow($"{ReadNoteWithTagsForUpdateAuthGetUrl}?id=1")]
    public async Task Api_Authorized_Get_ShouldReturns200(string uriString)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var statusCode = response.StatusCode;
        var reason = response.ReasonPhrase;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.OK));
    }

    [TestMethod]
    [DataRow($"{MigrationUploadPostUrl}", Request.Post, true)]
    [DataRow($"{CreateNotePostUrl}", Request.Post, false)]
    [DataRow($"{UpdateNotePutUrl}", Request.Put, false)]
    public async Task Api_Authorized_PostAndPut_ShouldReturns200(string uriString, Request requestMethod, bool appendFile)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);

        var uri = new Uri(uriString, UriKind.Relative);
        using var content = TestHelper.GetRequestContent(appendFile);

        // act:
        using var response = requestMethod == Request.Put
            ? await client.PutAsync(uri, content)
            : await client.PostAsync(uri, content);

        HttpStatusCode statusCode = response.StatusCode;
        string reason = response.ReasonPhrase;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);

        reason
            .Should()
            .Be(nameof(HttpStatusCode.OK));
    }
}

