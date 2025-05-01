using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using SearchEngine.Data.Dto;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations.Extensions;

/// <summary>
/// Контейнер с потрохами из тестов
/// </summary>
public static class TestHelper
{
    // константы с результатами запросов
    internal const string TagListResponse = "[\"Авторские: 1\",\"Бардовские\",\"Блюз: 1\",\"Народный стиль\",\"Вальсы\",\"Военные\",\"Военные (ВОВ)\",\"Гранж\",\"Дворовые\",\"Детские\"," +
                                            "\"Джаз\",\"Дуэты\",\"Зарубежные\",\"Застольные\",\"Авторские (Павел)\",\"Из мюзиклов\",\"Из фильмов\",\"Кавказские\",\"Классика\",\"Лирика\"," +
                                            "\"Медленные\",\"Народные\",\"Новогодние\",\"Панк\",\"Патриотические\",\"Песни 30х-60х\",\"Песни 60х-70х\",\"На стихи Есенина\",\"Поп-музыка\"," +
                                            "\"Походные\",\"Про водителей\",\"Про ГИБДД\",\"Про космонавтов\",\"Про милицию\",\"Ретро хиты\",\"Рождественские\",\"Рок\",\"Романсы\",\"Свадебные\"," +
                                            "\"Танго\",\"Танцевальные\",\"Шансон\",\"Шуточные\",\"Новые\"]";

    /// <summary>
    /// Добавить в контекст контроллера scoped контейнер со службами
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="serviceProvider"></param>
    internal static void AddHttpContext(this ControllerBase controller, IServiceProvider serviceProvider)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        };
    }

    /// <summary>
    /// Попытаться авторизоваться в сервисе, прикрепить куки к заголовкам в случае успеха
    /// </summary>
    internal static async Task TryAuthorizeToService(this HttpClient client, string login = "admin", string password = "admin")
    {
        var uri = new Uri($"account/login?email={login}&password={password}", UriKind.Relative);
        using var authResponse = await client.GetAsync(uri);
        authResponse.EnsureSuccessStatusCode();
        var headers = authResponse.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
    }

    /// <summary>
    /// Получить контент для POST запроса
    /// </summary>
    internal static dynamic GetRequestContent(bool appendFile)
    {
        var fileContent = new ByteArrayContent([0x1, 0x2, 0x3, 0x4]);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        var json = new NoteDto();
        var jsonContent = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        dynamic content = appendFile ? new MultipartFormDataContent() : jsonContent;
        if (appendFile) content.Add(fileContent, "file", "file.txt");

        return content;
    }

    internal static StringContent GetRequestContentWithTags()
    {
        // в TagsCheckedRequest содержатся отмеченные теги
        var json = new NoteDto { TagsCheckedRequest = Enumerable.Range(1, 44).ToList() };
        var jsonContent = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");

        return jsonContent;
    }

    internal static NoteDto GetNoteDto()
    {
        List<int> checkedTags = [1];
        var note = new NoteDto { TitleRequest = "тестовая запись", TextRequest = "раз два три", TagsCheckedRequest = checkedTags };
        return note;
    }

    internal static NoteDto GetNoteDto(List<int> tags)
    {
        var note = new NoteDto { TitleRequest = "название", TextRequest = "раз два три", TagsCheckedRequest = tags };
        return note;
    }

    internal static NoteDto GetNoteForUpdate(string text)
    {
        List<int> tagsForUpdate = [4];
        var noteForUpdate = new NoteDto { TitleRequest = "название", TextRequest = text, TagsCheckedRequest = tagsForUpdate };
        return noteForUpdate;
    }

    /// <summary>
    /// Очистить таблицы двух баз данных
    /// </summary>
    /// <param name="factory"></param>
    internal static void CleanUpDatabases(CustomWebAppFactory<IntegrationStartup> factory)
    {
        var pgConnectionString = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString(Startup.AdditionalConnectionKey);
        var mysqlConnectionString = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString(Startup.DefaultConnectionKey);

        using var pgConnection = new NpgsqlConnection(pgConnectionString);
        pgConnection.Open();
        var commands = new List<string>
        {
            // """TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "TagsToNotes" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Tag" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Note" RESTART IDENTITY CASCADE;"""
        };
        foreach (var command in commands)
        {
            using var cmd = new NpgsqlCommand(command, pgConnection);
            cmd.ExecuteNonQuery();
        }
        pgConnection.Close();

        using var mysqlConnection = new MySqlConnection(mysqlConnectionString);
        mysqlConnection.Open();
        commands =
        [
            "SET FOREIGN_KEY_CHECKS = 0;",
            // "TRUNCATE TABLE `Users`;",
            "TRUNCATE TABLE `Tag`;",
            "TRUNCATE TABLE `Note`;",
            "TRUNCATE TABLE `TagsToNotes`;",
            "SET FOREIGN_KEY_CHECKS = 1;",
        ];
        foreach (var command in commands)
        {
            using var cmd = new MySqlCommand(command, mysqlConnection);
            cmd.ExecuteNonQuery();
        }
        mysqlConnection.Close();
    }

}
