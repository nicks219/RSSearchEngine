using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    /// Получить первый элемент из списка релевантности для текста
    /// </summary>
    internal static async Task<int> GetFirstComplianceIndexFromTokenizer(this HttpClient client, string text)
    {
        using var complianceResponse = await client.GetAsync($"api/compliance/indices?text={text}");
        var result = await complianceResponse.Content.ReadAsStringAsync();
        var firstKey = JsonSerializer.Deserialize<ComplianceResponseModel>(result)?.res.Keys.ElementAt(0);
        Int32.TryParse(firstKey, out var complianceId);

        return complianceId;
    }

    /// <summary>
    /// Получить список тегов в формате "тег: количество заметок" либо "тег" если заметки в категории отсутствуют
    /// </summary>
    internal static async Task<List<string>> GetTagsFromReaderOnly(this HttpClient client)
    {
        using var tagsResponse = await client.GetAsync("api/create");
        var tagDto = await tagsResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<NoteDto>();
        var tags = tagDto.EnsureNotNull().StructuredTagsListResponse.EnsureNotNull();

        return tags;
    }

    /// <summary>
    /// Удалить заметку с требуемым идентификатором
    /// </summary>
    internal static async Task DeleteNoteFromService(this HttpClient client, int noteId)
    {
        // CatalogDto
        using var noteResponse = await client.GetAsync($"api/catalog?id={noteId}&pg=1");
        noteResponse.EnsureSuccessStatusCode();
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

    /// <summary>
    /// Получить контент для POST запроса
    /// </summary>
    internal static IEnumerable<StringContent> GetEnumeratedRequestContent(bool forUpdate = false)
    {
        var dto = new NoteDto { TitleRequest = "[1]", TextRequest = "посчитаем до четырёх", TagsCheckedRequest = [1] };
        var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        yield return jsonContent;

        var title = forUpdate ? "[1]" : "1";
        // todo: название заметки не очищается от скобочек, только именование тега
        dto = new NoteDto { TitleRequest = title, TextRequest = "раз два три четыре", TagsCheckedRequest = [1] };
        jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        yield return jsonContent;
    }

    internal static StringContent GetRequestContentWithTags()
    {
        // в TagsCheckedRequest содержатся отмеченные теги
        var dto = new NoteDto { TagsCheckedRequest = Enumerable.Range(1, 44).ToList() };
        var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

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
