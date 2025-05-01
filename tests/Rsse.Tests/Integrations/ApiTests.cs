using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations;

[TestClass]
public class ApiTests
{
    private readonly WebApplicationFactoryClientOptions _options = new() { BaseAddress = new Uri("http://localhost:5000/") };

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
        content.Values.First()!.ToString()
            .Should()
            .Be(Common.Constants.ApplicationFullName);
    }

    [TestMethod]
    [DataRow("api/read/title?id=1", "res", "Розенбаум -- Вечерняя застольная")]
    [DataRow("api/read/election", "randomElection", false)]
    [DataRow("api/read", "structuredTagsListResponse", TagListResponse)]// ReadTagList
    public async Task Api_ReadController_Get_ShouldReturnsExpectedResult(string uriString, string key, object expected)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        using var client = factory.CreateClient(_options);
        var uri = new System.Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var content = await response
            .EnsureSuccessStatusCode()
            .Content
            .ReadFromJsonAsync<System.Collections.Generic.Dictionary<string, object?>>();
        content.ThrowIfNull();
        var value = content[key] as JsonElement?;
        value.ThrowIfNull();

        // assert:
        switch (expected)
        {
            case string expectedAsString:
            {
                var actualAsString = value.Value.ToString();
                Assert.AreEqual(expectedAsString, actualAsString);
                break;
            }
            case bool expectedAsBool:
                var actualAsBool = value.Value.GetBoolean();
                Assert.AreEqual(expectedAsBool, actualAsBool);
                break;
        }
    }

    [TestMethod]
    [DataRow("api/read", "titleResponse", "Розенбаум -- Вечерняя застольная")]// GetNextOrSpecificNote
    public async Task Api_ReadController_Post_ShouldReturnsExpectedResult(string uriString, string key, string expected)
    {
        // arrange:
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        // в TagsCheckedRequest содержатся отмеченные теги
        var json = new NoteDto { TagsCheckedRequest = Enumerable.Range(1, 44).ToList() };
        var jsonContent = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        using var client = factory.CreateClient(_options);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.PostAsync(uri, jsonContent);
        var result = await response
            .EnsureSuccessStatusCode()
            .Content
            .ReadFromJsonAsync<Dictionary<string, object?>>();
        var structuredTagsListResponse = result!["structuredTagsListResponse"]!
            .ToString()!;
        var titleResponse = result[key]!
            .ToString()!;

        // assert:
        structuredTagsListResponse
            .Should()
            .BeEquivalentTo(TagListResponse);
        titleResponse
            .Should()
            .BeEquivalentTo(expected);
    }

    // константы с результатами запросов
    private const string TagListResponse = "[\"Авторские: 1\",\"Бардовские\",\"Блюз: 1\",\"Народный стиль\",\"Вальсы\",\"Военные\",\"Военные (ВОВ)\",\"Гранж\",\"Дворовые\",\"Детские\"," +
                                           "\"Джаз\",\"Дуэты\",\"Зарубежные\",\"Застольные\",\"Авторские (Павел)\",\"Из мюзиклов\",\"Из фильмов\",\"Кавказские\",\"Классика\",\"Лирика\"," +
                                           "\"Медленные\",\"Народные\",\"Новогодние\",\"Панк\",\"Патриотические\",\"Песни 30х-60х\",\"Песни 60х-70х\",\"На стихи Есенина\",\"Поп-музыка\"," +
                                           "\"Походные\",\"Про водителей\",\"Про ГИБДД\",\"Про космонавтов\",\"Про милицию\",\"Ретро хиты\",\"Рождественские\",\"Рок\",\"Романсы\",\"Свадебные\"," +
                                           "\"Танго\",\"Танцевальные\",\"Шансон\",\"Шуточные\",\"Новые\"]";
}
