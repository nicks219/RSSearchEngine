using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Rsse.Domain.Service.ApiModels;
using Rsse.Domain.Service.Configuration;

namespace Rsse.Tests.Integration.FakeDb.Extensions;

/// <summary>
/// Контейнер с вспомогательным функционалом для тестов.
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
        var json = new NoteRequest { Title = "<unk>", Text = string.Empty };
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
    /// <param name="mediaTypeOnly">Добавить только тип контента.</param>
    internal static void EnrichDataIfNecessary(HttpRequestMessage httpRequest, bool mediaTypeOnly = false)
    {
        var uriStr = httpRequest.RequestUri?.OriginalString;
        switch (uriStr)
        {
            case RouteConstants.CatalogNavigatePostUrl:
                {
                    var json = mediaTypeOnly ? "" : JsonSerializer.Serialize(new CatalogRequest(0, [1]));
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    return;
                }

            case RouteConstants.MigrationUploadPostUrl:
                {
                    var fileContent = new ByteArrayContent([0x1, 0x2, 0x3, 0x4]);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    var multipartFormDataContent = new MultipartFormDataContent();
                    if (!mediaTypeOnly) multipartFormDataContent.Add(fileContent, "file", "file.txt");
                    httpRequest.Content = multipartFormDataContent;
                    return;
                }

            case RouteConstants.CreateNotePostUrl:
            case RouteConstants.UpdateNotePutUrl:
                {
                    var json = mediaTypeOnly ? "" : JsonSerializer.Serialize(new NoteRequest());
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    return;
                }

            default: return;
        }
    }
}
