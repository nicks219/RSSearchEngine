using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.Configuration;
using SearchEngine.Tests.Integrations.Api;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations;

[TestClass]
public class ApiTests
{
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private readonly WebApplicationFactoryClientOptions _options = new() { BaseAddress = BaseAddress };

    /// <summary>
    /// Запустить отложенную очистку файлов бд sqlite (на windows) в финале тестовой сборки
    /// </summary>
    [ClassCleanup(ClassCleanupBehavior.EndOfAssembly)]
    public static void CleanUp()
    {
        SqliteFileCleaner.ScheduleFileDeletionWindowsOnly();
    }

    [TestMethod]
    public async Task Api_SystemController_Get_ReturnsResult()
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        using var client = factory.CreateClient(_options);
        var uri = new Uri("system/version", UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var content = await response
            .Content
            .ReadFromJsonAsync<Dictionary<string, object?>>();

        // assert:
        content
            .Should()
            .NotBeNull();

        content.Values.First()
            .EnsureNotNull()
            .ToString()
            .Should()
            .Be(Constants.ApplicationFullName);
    }

    [TestMethod]
    [DataRow("api/read/title?id=1", "res", "Розенбаум -- Вечерняя застольная")]
    [DataRow("api/read/election", "randomElection", false)]
    [DataRow("api/read", "structuredTagsListResponse", TestHelper.TagListResponse)]
    public async Task Api_ReadController_Get_ShouldReturnsExpectedResult(string uriString, string key, object expected)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        using var client = factory.CreateClient(_options);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var content = await response
            .EnsureSuccessStatusCode()
            .Content
            .ReadFromJsonAsync<Dictionary<string, object?>>();

        content.EnsureNotNull();
        var value = content[key] as JsonElement?;
        value.EnsureNotNull();

        // assert:
        switch (expected)
        {
            case string expectedAsString:
                {
                    var actualAsString = value.Value.ToString();
                    actualAsString
                        .Should()
                        .Be(expectedAsString);

                    break;
                }

            case bool expectedAsBool:
                var actualAsBool = value.Value.GetBoolean();
                actualAsBool
                    .Should()
                    .Be(expectedAsBool);

                break;
        }
    }

    [TestMethod]
    [DataRow("api/read", "titleResponse", "Розенбаум -- Вечерняя застольная")]
    public async Task Api_ReadController_Post_ShouldReturnsExpectedResult(string uriString, string key, string expected)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        using var content = TestHelper.GetRequestContentWithTags();
        using var client = factory.CreateClient(_options);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.PostAsync(uri, content);
        var result = await response
            .EnsureSuccessStatusCode()
            .Content
            .ReadFromJsonAsync<Dictionary<string, object?>>();

        var structuredTagsListResponse = result
            .EnsureNotNull()
            .GetValueOrDefault("structuredTagsListResponse")
            .EnsureNotNull()
            .ToString()
            .EnsureNotNull();

        var titleResponse = result[key]
            .EnsureNotNull()
            .ToString()
            .EnsureNotNull();

        // assert:
        structuredTagsListResponse
            .Should()
            .BeEquivalentTo(TestHelper.TagListResponse);

        titleResponse
            .Should()
            .BeEquivalentTo(expected);
    }
}
