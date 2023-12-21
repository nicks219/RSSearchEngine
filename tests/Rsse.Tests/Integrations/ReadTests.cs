using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations;

[TestClass]
public class ReadTests
{
    [TestMethod]
    public async Task Integration_ReadNoteTitleById_ShouldPassCorrectly()
    {
        // NB: для тестов скриптом создаётся SQLite бд в файле, с одной песней

        // arrange:
        var factory = new CustomWebAppFactory();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };

        // act:
        var client = factory.CreateClient(options);
        // election: var uri = new Uri("api/read/election", UriKind.Relative);
        var uri = new Uri("api/read/title?id=1", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var status = response.ReasonPhrase;
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // assert:
        response.Should().NotBeNull();
        status.Should().Be("OK");
        content.Should().NotBeNull();
        content!.Values.First().Should().Be("Розенбаум - Вечерняя застольная");
    }
}
