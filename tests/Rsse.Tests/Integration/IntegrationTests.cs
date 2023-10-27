using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchEngine.Tests.Integration;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public async Task SimpleHttpCall_Test()
    {
        // arrange:
        var factory = new CustomWebAppFactory();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };

        // act:
        var client = factory.CreateClient(options);
        var uri = new Uri("api/read/election", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var status = response.ReasonPhrase;

        // assert:
        response.Should().NotBeNull();
        status.Should().Be("OK");
    }
}
