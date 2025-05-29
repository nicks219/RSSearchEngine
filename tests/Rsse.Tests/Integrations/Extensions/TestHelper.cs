using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using SearchEngine.Api.Startup;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;
using SearchEngine.Tests.Integrations.IntegrationTests.RealDb;

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
        var dto = new NoteRequest { CheckedTags = Enumerable.Range(1, 44).ToList() };
        var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        return jsonContent;
    }

    /// <summary>
    /// Очистить таблицы двух баз данных
    /// </summary>
    /// <param name="factory">Хост.</param>
    /// <param name="ct">Токен отмены.</param>
    // todo: перенести в сид если требуется очистка
    internal static async Task CleanUpDatabases(WebApplicationFactory<Startup> factory, CancellationToken ct)
    {
        var pgConnectionString = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString(Startup.AdditionalConnectionKey);
        var mysqlConnectionString = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString(Startup.DefaultConnectionKey);

        await using var pgConnection = new NpgsqlConnection(pgConnectionString);
        await pgConnection.OpenAsync(ct);
        var commands = new List<string>
        {
            // """TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "TagsToNotes" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Tag" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Note" RESTART IDENTITY CASCADE;"""
        };
        foreach (var command in commands)
        {
            await using var cmd = new NpgsqlCommand(command, pgConnection);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using var mysqlConnection = new MySqlConnection(mysqlConnectionString);
        await mysqlConnection.OpenAsync(ct);
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
            await using var cmd = new MySqlCommand(command, mysqlConnection);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    /// <summary>
    /// Вернуть ожидаемое исключение, которое вызовет выполнение асинхронного метода.
    /// </summary>
    /// <param name="action">Асинхронный метод.</param>
    /// <typeparam name="T">Тип исключения.</typeparam>
    /// <returns>Исключение либо <b>null</b></returns>
    internal static async Task<T?> GetExpectedExceptionWithAsync<T>(Func<Task> action) where T : Exception
    {
        T? exception = null;
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (T ex)
        {
            exception = ex;
        }

        return exception;
    }

    /// <summary>
    /// Добавить в запрос тестовую информацию при необходимости.
    /// </summary>
    /// <param name="httpRequest">Запрос.</param>
    internal static void EnrichDataIfNecessary(HttpRequestMessage httpRequest)
    {
        var uriStr = httpRequest.RequestUri?.OriginalString;
        switch (uriStr)
        {
            case RouteConstants.CatalogNavigatePostUrl:
                {
                    var json = JsonSerializer.Serialize(new CatalogRequest(0, [1]));
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    return;
                }

            case RouteConstants.MigrationUploadPostUrl:
                {
                    var fileContent = new ByteArrayContent([0x1, 0x2, 0x3, 0x4]);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    var data = new MultipartFormDataContent();
                    data.Add(fileContent, "file", "file.txt");
                    httpRequest.Content = data;
                    return;
                }

            case RouteConstants.CreateNotePostUrl:
            case RouteConstants.UpdateNotePutUrl:
                {
                    var json = JsonSerializer.Serialize(new NoteRequest());
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    return;
                }

            default: return;
        }
    }
}

/// <summary>
/// Глагол http вызова.
/// </summary>
public enum Method
{
    Get = 0,
    Post = 1,
    Delete = 2,
    Put = 3
}
