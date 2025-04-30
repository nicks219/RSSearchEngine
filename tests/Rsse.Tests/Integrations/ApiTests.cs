using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// <summary>
    /// Запустить отложенную очистку файлов бд sqlite (на windows) в финале тестовой сборки
    /// </summary>
    [ClassCleanup(ClassCleanupBehavior.EndOfAssembly)]
    public static void CleanUp()
    {
        SqliteFileCleaner.ScheduleFileDeletionWindowsOnly();
    }

    [TestMethod]
    [DataRow("api/read/title?id=1", "res", "Розенбаум -- Вечерняя застольная")]
    [DataRow("api/read/election", "randomElection", false)]
    [DataRow("api/read", "structuredTagsListResponse", TagListResponse)]// ReadTagList
    public async Task Api_ReadController_Get_ShouldReturnsExpectedResult(string uriString, string key, object expected)
    {
        // arrange:
        var baseUri = new Uri("http://localhost:5000/");
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        var options = new WebApplicationFactoryClientOptions { BaseAddress = baseUri };
        using var client = factory.CreateClient(options);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.GetAsync(uri);
        var status = response.ReasonPhrase;
        dynamic contentTask = expected is string
            ? response.Content.ReadFromJsonAsync<Dictionary<string, object?>>()
            : response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();

        // assert:
        response.EnsureSuccessStatusCode();
        status.Should().Be(HttpStatusCode.OK.ToString());
        var content = await contentTask;
        switch (expected)
        {
            case string expectedAsString:
            {
                var actualAsString = (string)content[key].ToString();
                Assert.AreEqual(expectedAsString, actualAsString);
                break;
            }
            case bool expectedAsBool:
                Assert.AreEqual(expectedAsBool, content[key]);
                break;
        }
    }

    [TestMethod]
    [DataRow("api/read", "titleResponse", "Розенбаум -- Вечерняя застольная")]// GetNextOrSpecificNote
    public async Task Api_ReadController_Post_ShouldReturnsExpectedResult(string uriString, string key, string expected)
    {
        // arrange:
        var baseUri = new Uri("http://localhost:5000/");
        await using var factory = new CustomWebAppFactory<SqliteApiStartup>();
        var options = new WebApplicationFactoryClientOptions { BaseAddress = baseUri };
        // в TagsCheckedRequest содержатся отмеченные теги
        var json = new NoteDto
        {
            TagsCheckedRequest = Enumerable.Range(1, 44).ToList()
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        using var client = factory!.CreateClient(options!);
        var uri = new Uri(uriString, UriKind.Relative);

        // act:
        using var response = await client.PostAsync(uri, jsonContent);
        var reason = response.ReasonPhrase;
        var statusCode = response.StatusCode;
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>();
        var structuredTagsListResponse = result!["structuredTagsListResponse"]!.ToString()!;
        var titleResponse = result[key]!.ToString()!;

        // assert:
        statusCode.Should().Be(HttpStatusCode.OK);
        reason.Should().Be(HttpStatusCode.OK.ToString());

        structuredTagsListResponse.Should().BeEquivalentTo(TagListResponse);
        titleResponse.Should().BeEquivalentTo(expected);
    }

    // константы с результатами запросов
    private const string TagListResponse = "[\"Авторские: 1\",\"Бардовские\",\"Блюз: 1\",\"Народный стиль\",\"Вальсы\",\"Военные\",\"Военные (ВОВ)\",\"Гранж\",\"Дворовые\",\"Детские\"," +
                                           "\"Джаз\",\"Дуэты\",\"Зарубежные\",\"Застольные\",\"Авторские (Павел)\",\"Из мюзиклов\",\"Из фильмов\",\"Кавказские\",\"Классика\",\"Лирика\"," +
                                           "\"Медленные\",\"Народные\",\"Новогодние\",\"Панк\",\"Патриотические\",\"Песни 30х-60х\",\"Песни 60х-70х\",\"На стихи Есенина\",\"Поп-музыка\"," +
                                           "\"Походные\",\"Про водителей\",\"Про ГИБДД\",\"Про космонавтов\",\"Про милицию\",\"Ретро хиты\",\"Рождественские\",\"Рок\",\"Романсы\",\"Свадебные\"," +
                                           "\"Танго\",\"Танцевальные\",\"Шансон\",\"Шуточные\",\"Новые\"]";
}
