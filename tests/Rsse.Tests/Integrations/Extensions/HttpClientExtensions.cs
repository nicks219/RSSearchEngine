using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Tests.Integrations.Dto;
using static SearchEngine.Domain.Configuration.RouteConstants;

namespace SearchEngine.Tests.Integrations.Extensions;

/// <summary>
/// Расширения для запросов на различные ручки сервиса
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Попытаться авторизоваться в сервисе, прикрепить куки к заголовкам в случае успеха
    /// </summary>
    internal static async Task TryAuthorizeToService(this HttpClient client, string login = "admin", string password = "admin")
    {
        var uri = new Uri($"{AccountLoginGetUrl}?email={login}&password={password}", UriKind.Relative);
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
        using var complianceResponse = await client.GetAsync($"{ComplianceIndicesGetUrl}?text={text}");
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
        using var tagsResponse = await client.GetAsync($"{ReadTagsForCreateAuthGetUrl}");
        var tagDto = await tagsResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<NoteResponse>();
        var tags = tagDto.EnsureNotNull().StructuredTagsListResponse.EnsureNotNull();

        return tags;
    }

    /// <summary>
    /// Удалить заметку с требуемым идентификатором
    /// </summary>
    internal static async Task DeleteNoteFromService(this HttpClient client, int noteId)
    {
        // CatalogDto
        using var noteResponse = await client.DeleteAsync($"{NoteDeleteUrl}?id={noteId}&pg=1");
        noteResponse.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Создать требуемое количество случайных записей в базк
    /// </summary>
    /// <param name="client"></param>
    /// <param name="notes"></param>
    internal static async Task CreateNotes(this HttpClient client, int notes)
    {
        for (var i = 0; i < notes; i++)
        {
            var guid = Guid.NewGuid();
            var createRequest = new NoteRequest { TitleRequest = $"название: {guid}", TextRequest = $"текст: {guid}", TagsCheckedRequest = [1] };
            using var content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            using var _ = await client.PostAsync($"{CreateNotePostUrl}", content);
        }
    }

    /// <summary>
    /// Инициализировать контекст контроллера scoped контейнером со службами
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
}
