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
public class AuthTests
{
    [TestMethod]
    public async Task UnauthenticatedDeleteCall_ShouldRejected()
    {
        // arrange:
        var factory = new CustomWebAppFactory<AuthStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };

        // act:
        var client = factory.CreateClient(options);
        var uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);
        var response = await client.DeleteAsync(uri);
        var status = response.ReasonPhrase;
        var headers = response.Headers;

        // assert:
        status.Should().Be("Unauthorized");
        response.Should().NotBeNull();
        var shift = headers.FirstOrDefault(e => e.Key == Constants.ShiftHeaderName);
        shift.Value.First().Should().Be(Constants.ShiftHeaderValue);
    }

    [TestMethod]
    public async Task UnauthorizedDeleteCall_ShouldBeForbidden()
    {
        // arrange:
        var factory = new CustomWebAppFactory<AuthStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri,
            HandleCookies = true
        };

        // act: login:
        var client = factory.CreateClient(options);
        var uri = new Uri("account/login?email=editor&password=editor", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();

        // act: request:
        uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
        response = await client.DeleteAsync(uri);
        var status = response.ReasonPhrase;
        var content = await response.Content.ReadAsStringAsync();

        // assert:
        status.Should().Be("Forbidden");
        content.Should().NotBeNull();
        content.Should().Be("GET: access denied.");
    }

    [TestMethod]
    public async Task AuthorizedDeleteCall_ShouldPass()
    {
        // arrange:
        var factory = new CustomWebAppFactory<AuthStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri,
            HandleCookies = true
        };

        // act: login:
        var client = factory.CreateClient(options);
        var uri = new Uri("account/login?email=admin&password=admin", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var headers = response.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();

        // act: request:
        uri = new Uri("api/catalog?id=1&pg=1", UriKind.Relative);
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
        response = await client.DeleteAsync(uri);
        var status = response.ReasonPhrase;

        // assert:
        status.Should().Be("OK"); ;
    }
}
