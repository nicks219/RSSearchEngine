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
public class ApiSimpleTests
{
    [TestMethod]
    public async Task Api_ReadTitleByNoteId_ReturnsTitle()
    {
        // NB: для тестов скриптом создаётся SQLite бд в файле, с одной песней

        // arrange:
        await using var factory = new CustomWebAppFactory<SimpleMirrorStartup>();
        var baseUri = new Uri("http://localhost:5000/");
        var options = new WebApplicationFactoryClientOptions { BaseAddress = baseUri };

        // act:
        using var client = factory.CreateClient(options);
        var uri = new Uri("api/read/title?id=1", UriKind.Relative);
        using var response = await client.GetAsync(uri);
        var status = response.ReasonPhrase;
        var contentTask = response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // assert:
        response.EnsureSuccessStatusCode();
        status.Should().Be("OK");
        var content = await contentTask;
        content.Should().NotBeNull();
        content!.Values.First().Should().Be("Розенбаум -- Вечерняя застольная");
    }
}
