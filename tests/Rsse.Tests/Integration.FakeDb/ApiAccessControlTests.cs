using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Api.Startup;
using Rsse.Domain.Service.Configuration;
using Rsse.Tests.Integration.FakeDb.Api;
using Rsse.Tests.Integration.FakeDb.Extensions;
using static Rsse.Domain.Service.Configuration.RouteConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Rsse.Tests.Integration.FakeDb;

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

    // c MSTest.TestFramework 4.0.0 убрали ClassCleanupBehavior
    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void CleanUp() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthorized_Delete_ShouldReturns401()
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
    public async Task Unauthenticated_Delete_ShouldReturns403()
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
    public async Task Unauthorized_Get_ShouldReturns401(string uriString)
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
    [DataRow($"{MigrationUploadPostUrl}", Method.Post)]
    [DataRow($"{CreateNotePostUrl}", Method.Post)]
    [DataRow($"{UpdateNotePutUrl}", Method.Put)]
    public async Task Unauthorized_PostAndPut_ShouldReturns401(string uriString, Method requestMethod)
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
    public async Task Authorized_Delete_ShouldReturns200()
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        // запрос на удаление несуществующей заметки, чтобы не аффектить тесты, завязанные на её чтение
        var uri = new Uri($"{DeleteNoteUrl}?id=2&pg=1", UriKind.Relative);

        // act:
        using var response = await client.DeleteAsync(uri);
        var statusCode = response.StatusCode;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [TestMethod]
    [DataRow($"{MigrationDownloadGetUrl}?filename=backup_9.dump")]
    [DataRow($"{AccountCheckGetUrl}")]
    [DataRow($"{AccountUpdateGetUrl}?OldCredos.Email=admin&OldCredos.Password=admin&NewCredos.Email=admin&NewCredos.Password=admin")]
    [DataRow($"{ReadTagsForCreateAuthGetUrl}")]
    [DataRow($"{ReadNoteWithTagsForUpdateAuthGetUrl}?id=1")]
    public async Task Authorized_Get_ShouldReturns200(string uriString)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var statusCode = response.StatusCode;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [TestMethod]
    [DataRow($"{MigrationUploadPostUrl}", Method.Post, true)]
    [DataRow($"{CreateNotePostUrl}", Method.Post, false)]
    [DataRow($"{UpdateNotePutUrl}", Method.Put, false)]
    public async Task Authorized_PostAndPut_ShouldReturns200(string uriString, Method requestMethod, bool appendFile)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);

        var uri = new Uri(uriString, UriKind.Relative);
        using var content = TestHelper.GetRequestContent(appendFile);

        // act:
        using var response = requestMethod == Method.Put
            ? await client.PutAsync(uri, content)
            : await client.PostAsync(uri, content);

        HttpStatusCode statusCode = response.StatusCode;

        // assert:
        statusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    public static IEnumerable<object[]> CancellationUnauthorizedTestData =>
    [
        [AccountCheckGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [AccountUpdateGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [ReadTagsForCreateAuthGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [ReadNoteWithTagsForUpdateAuthGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [MigrationCopyGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [MigrationCreateGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [MigrationRestoreGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [MigrationDownloadGetUrl, HttpMethod.Get, HttpStatusCode.Unauthorized],
        [CreateNotePostUrl, HttpMethod.Post, HttpStatusCode.Unauthorized],
        [MigrationUploadPostUrl, HttpMethod.Post, HttpStatusCode.Unauthorized],
        [DeleteNoteUrl, HttpMethod.Delete, HttpStatusCode.Unauthorized],
        [UpdateNotePutUrl, HttpMethod.Put, HttpStatusCode.Unauthorized],

        [$"{AccountLoginGetUrl}?email=.&password=.", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [AccountLogoutGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{CatalogPageGetUrl}?id=0", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ComplianceIndicesGetUrl}?text=.", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [SystemVersionGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ReadTitleGetUrl}?id=1", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [ReadTagsGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ReadElectionGetUrl}?electionType=RoundRobin", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [SystemWaitWarmUpGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [ReadNotePostUrl, HttpMethod.Post, HttpStatusCode.ServiceUnavailable]
    ];

    [TestMethod]
    [DynamicData(nameof(CancellationUnauthorizedTestData))]
    public async Task CancelledRequest_UnauthorizedCall_ShouldProducesExpectedStatus(string uriString, HttpMethod method, HttpStatusCode expected)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        var ct = new CancellationToken(canceled: true);
        using var request = new HttpRequestMessage(method, uri);
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(expected);
    }

    private const string AccountUpdateRequest =
        $"{AccountUpdateGetUrl}?OldCredos.Email=admin&OldCredos.Password=admin&NewCredos.Email=admin&NewCredos.Password=admin";

    // играет роль очередность запросов тк один хост
    public static IEnumerable<object[]> CancellationAuthorizedTestData =>
    [
        [AccountLogoutGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{AccountLoginGetUrl}?email=.&password=.", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],// exc -> middleware 401
        [AccountCheckGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [ReadTagsForCreateAuthGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ReadNoteWithTagsForUpdateAuthGetUrl}?id=0", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{CatalogPageGetUrl}?id=0", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ComplianceIndicesGetUrl}?text=.", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [SystemVersionGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ReadTitleGetUrl}?id=1", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [ReadTagsGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [$"{ReadElectionGetUrl}?electionType=RoundRobin", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [SystemWaitWarmUpGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [ReadNotePostUrl, HttpMethod.Post, HttpStatusCode.ServiceUnavailable],
        [$"{MigrationDownloadGetUrl}?filename=1", HttpMethod.Get, HttpStatusCode.ServiceUnavailable],// 404
        [AccountUpdateRequest, HttpMethod.Get, HttpStatusCode.ServiceUnavailable]
    ];

    [TestMethod]
    [DynamicData(nameof(CancellationAuthorizedTestData))]
    public async Task CancelledRequest_AuthorizedCall_ShouldProducesExpectedStatus(string uriString, HttpMethod method, HttpStatusCode expected)
    {
        // arrange:
        using var client = _factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        var ct = new CancellationToken(canceled: true);
        using var request = new HttpRequestMessage(method, uri);
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(expected);
    }

    public static IEnumerable<object[]> CancellationHostTestData =>
    [
        [$"{AccountLoginGetUrl}?email=.&password=.", HttpMethod.Get, HttpStatusCode.Unauthorized],
        [MigrationCreateGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [MigrationCopyGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],// 500: sqlite not supported
        [MigrationRestoreGetUrl, HttpMethod.Get, HttpStatusCode.ServiceUnavailable],
        [MigrationUploadPostUrl, HttpMethod.Post, HttpStatusCode.ServiceUnavailable],
        [CreateNotePostUrl, HttpMethod.Post, HttpStatusCode.ServiceUnavailable],
        [UpdateNotePutUrl, HttpMethod.Put, HttpStatusCode.ServiceUnavailable],
        [$"{DeleteNoteUrl}?id=0&pg=0", HttpMethod.Delete, HttpStatusCode.ServiceUnavailable]
    ];

    [TestMethod]
    [DynamicData(nameof(CancellationHostTestData))]
    public async Task CancelledHost_AuthorizeCall_ShouldProducesExpectedStatus(string uriString, HttpMethod method, HttpStatusCode expected)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<Startup>();
        using var client = factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        var ct = CancellationToken.None;
        using var request = new HttpRequestMessage(method, uri);
        TestHelper.EnrichDataIfNecessary(request);
        _ = factory.HostInternal.EnsureNotNull().StopAsync(CancellationToken.None);
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(expected);
    }

    public static IEnumerable<object[]> BadRequestTestData =>
    [
        [AccountLoginGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [AccountUpdateGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [DeleteNoteUrl, HttpMethod.Delete, HttpStatusCode.BadRequest],
        [CatalogPageGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [ComplianceIndicesGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [MigrationDownloadGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [ReadTitleGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [ReadNoteWithTagsForUpdateAuthGetUrl, HttpMethod.Get, HttpStatusCode.BadRequest],
        [MigrationUploadPostUrl, HttpMethod.Post, HttpStatusCode.BadRequest],
        [CatalogNavigatePostUrl, HttpMethod.Post, HttpStatusCode.BadRequest],
        [CreateNotePostUrl, HttpMethod.Post, HttpStatusCode.BadRequest],
        [UpdateNotePutUrl, HttpMethod.Put, HttpStatusCode.BadRequest],
    ];

    [TestMethod]
    [DynamicData(nameof(BadRequestTestData))]
    public async Task EmptyParameters_AuthorizeCall_ShouldProduceBadRequests(string uriString, HttpMethod method, HttpStatusCode expected)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<Startup>();
        using var client = factory.CreateClient(_options);
        await client.TryAuthorizeToService(ct: Token);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        var ct = CancellationToken.None;
        using var request = new HttpRequestMessage(method, uri);
        TestHelper.EnrichDataIfNecessary(request, mediaTypeOnly: true);
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var statusCode = response.StatusCode;

        // assert:
        statusCode.Should().Be(expected);
    }
}

