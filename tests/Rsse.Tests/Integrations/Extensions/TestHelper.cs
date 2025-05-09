using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using SearchEngine.Api.Startup;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Managers;
using SearchEngine.Tests.Integrations.Api;

namespace SearchEngine.Tests.Integrations.Extensions;

/// <summary>
/// Контейнер с потрохами из тестов
/// </summary>
// todo: отрефакторить
public static class TestHelper
{
    // константы с результатами запросов
    internal const string TagListResponse = "[\"Авторские: 1\",\"Бардовские\",\"Блюз: 1\",\"Народный стиль\",\"Вальсы\",\"Военные\",\"Военные (ВОВ)\",\"Гранж\",\"Дворовые\",\"Детские\"," +
                                            "\"Джаз\",\"Дуэты\",\"Зарубежные\",\"Застольные\",\"Авторские (Павел)\",\"Из мюзиклов\",\"Из фильмов\",\"Кавказские\",\"Классика\",\"Лирика\"," +
                                            "\"Медленные\",\"Народные\",\"Новогодние\",\"Панк\",\"Патриотические\",\"Песни 30х-60х\",\"Песни 60х-70х\",\"На стихи Есенина\",\"Поп-музыка\"," +
                                            "\"Походные\",\"Про водителей\",\"Про ГИБДД\",\"Про космонавтов\",\"Про милицию\",\"Ретро хиты\",\"Рождественские\",\"Рок\",\"Романсы\",\"Свадебные\"," +
                                            "\"Танго\",\"Танцевальные\",\"Шансон\",\"Шуточные\",\"Новые\"]";

    /// <summary>
    /// Получить контент для POST запроса
    /// </summary>
    internal static dynamic GetRequestContent(bool appendFile)
    {
        var fileContent = new ByteArrayContent([0x1, 0x2, 0x3, 0x4]);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        var json = new NoteRequest();
        var jsonContent = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        dynamic content = appendFile ? new MultipartFormDataContent() : jsonContent;
        if (appendFile) content.Add(fileContent, "file", "file.txt");

        return content;
    }

    internal static StringContent GetRequestContentWithTags()
    {
        // в TagsCheckedRequest содержатся отмеченные теги
        var dto = new NoteRequest { TagsCheckedRequest = Enumerable.Range(1, 44).ToList() };
        var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        return jsonContent;
    }

    /// <summary>
    /// Очистить таблицы двух баз данных
    /// </summary>
    /// <param name="factory"></param>
    // todo: перенести в сид если требуется очистка
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


    // для сценарных тестов
    private static readonly NoteRequest CreateRequest = new() { TitleRequest = "[1]", TextRequest = "посчитаем до четырёх", TagsCheckedRequest = [1] };
    private static readonly NoteRequest UpdateRequest = new() { TitleRequest = "[1]", TextRequest = "раз два три четыре", TagsCheckedRequest = [1], NoteIdExchange = 946 };
    private static readonly NoteRequest ReadRequest = new() { TagsCheckedRequest = [1] };
    private static readonly CatalogRequest CatalogRequest = new(PageNumber: 1, Direction: [CatalogManager.Forward]);
    public static StringContent CreateContent => new(JsonSerializer.Serialize(CreateRequest), Encoding.UTF8, "application/json");
    public static StringContent UpdateContent => new(JsonSerializer.Serialize(UpdateRequest), Encoding.UTF8, "application/json");
    public static StringContent ReadContent => new(JsonSerializer.Serialize(ReadRequest), Encoding.UTF8, "application/json");
    public static StringContent CatalogContent => new(JsonSerializer.Serialize(CatalogRequest), Encoding.UTF8, "application/json");
    public static StringContent Empty => new("");
}

// <summary/> тип http вызова
public enum Request
{
    Get = 0,
    Post = 1,
    Delete = 2,
    Put = 3
}
