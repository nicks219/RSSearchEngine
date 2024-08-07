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
public class ApiReadTests
{
    [TestMethod]
    public async Task ReadNoteTitleById_ShouldPassCorrectly()
    {
        // NB: для тестов скриптом создаётся SQLite бд в файле, с одной песней

        // arrange:
        var factory = new CustomWebAppFactory<SimpleStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseUri
        };

        // act:
        var client = factory.CreateClient(options);
        var uri = new Uri("api/read/title?id=1", UriKind.Relative);
        var response = await client.GetAsync(uri);
        var status = response.ReasonPhrase;
        var contentTask = response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // assert:
        status.Should().Be("OK");
        response.Should().NotBeNull();
        var content = await contentTask;
        content.Should().NotBeNull();
        content!.Values.First().Should().Be("Розенбаум - Вечерняя застольная");
    }
}
