using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Startup;
using SearchEngine.Domain.Configuration;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Extensions;
using static SearchEngine.Domain.Configuration.RouteConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace SearchEngine.Tests.Integrations;

/// <summary>
/// Тесты аутентификации и авторизации.
/// </summary>
[TestClass]
public class ApiAccessControlTests
{
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
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
        var uri = new Uri($"{Catalog}?id=1&pg=1", UriKind.Relative);
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
            .Be(HttpStatusCode.Unauthorized.ToString());

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
        await client.TryAuthorizeToService(unauthenticated, unauthenticated);
        var uri = new Uri($"{Catalog}?id=1&pg=1", UriKind.Relative);

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
            .Be(HttpStatusCode.Forbidden.ToString());

        content
            .Should()
            .Be("GET: access denied.");
    }

    [TestMethod]
    [DataRow($"{Migration}/{MigrationCopyGetUrl}")]
    [DataRow($"{Migration}/{MigrationCreateGetUrl}?fileName=123&databaseType=MySql")]
    [DataRow($"{Migration}/{MigrationRestoreGetUrl}?fileName=123&databaseType=MySql")]
    [DataRow($"{Migration}/{MigrationDownloadGetUrl}?filename=123")]
    [DataRow($"{Account}/{AccountCheckGetUrl}")]
    [DataRow($"{Account}/{AccountUpdateGetUrl}?OldCredos.Email=1&OldCredos.Password=2&NewCredos.Email=3&NewCredos.Password=4")]
    [DataRow($"{Create}")]
    [DataRow($"{Update}?id=1")]
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
            .Be(HttpStatusCode.Unauthorized.ToString());

        shift.Value.First()
            .Should()
            .Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    [DataRow($"{Migration}/{MigrationUploadPostUrl}")]
    [DataRow($"{Create}")]
    [DataRow($"{Update}")]
    public async Task Api_Unauthorized_Post_ShouldReturns401(string uriString)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        using MultipartFormDataContent content = TestHelper.GetRequestContent(true);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.PostAsync(uri, content);
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
            .Be(HttpStatusCode.Unauthorized.ToString());

        shift.Value.First()
            .Should()
            .Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task Api_Authorized_Delete_ShouldReturns200()
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService();
        // запрос на удаление несуществующей заметки, чтобы не аффектить тесты, завязанные на её чтение
        var uri = new Uri($"{Catalog}?id=2&pg=1", UriKind.Relative);

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
            .Be(HttpStatusCode.OK.ToString());
    }

    [TestMethod]
    [DataRow($"{Migration}/{MigrationDownloadGetUrl}?filename=backup_9.dump")]
    [DataRow($"{Account}/{AccountCheckGetUrl}")]
    [DataRow($"{Account}/{AccountUpdateGetUrl}?OldCredos.Email=admin&OldCredos.Password=admin&NewCredos.Email=admin&NewCredos.Password=admin")]
    [DataRow($"{Create}")]
    [DataRow($"{Update}?id=1")]
    public async Task Api_Authorized_Get_ShouldReturns200(string uriString)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService();
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
            .Be(HttpStatusCode.OK.ToString());
    }

    [TestMethod]
    [DataRow($"{Migration}/{MigrationUploadPostUrl}", true)]
    [DataRow($"{Create}", false)]
    [DataRow($"{Update}", false)]
    public async Task Api_Authorized_Post_ShouldReturns200(string uriString, bool appendFile)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService();

        var uri = new Uri(uriString, UriKind.Relative);
        using var content = TestHelper.GetRequestContent(appendFile);

        // act:
        using var response = await client.PostAsync(uri, content);
        HttpStatusCode statusCode = response.StatusCode;
        string reason = response.ReasonPhrase;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);

        reason
            .Should()
            .Be(HttpStatusCode.OK.ToString());
    }
}

