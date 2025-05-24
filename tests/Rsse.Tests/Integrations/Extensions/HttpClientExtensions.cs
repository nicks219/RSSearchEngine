using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static SearchEngine.Service.Configuration.RouteConstants;

namespace SearchEngine.Tests.Integrations.Extensions;

/// <summary>
/// Расширения для запросов на различные ручки сервиса
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Попытаться авторизоваться в сервисе, прикрепить куки к заголовкам в случае успеха
    /// </summary>
    internal static async Task TryAuthorizeToService(
        this HttpClient client,
        string login = "admin",
        string password = "admin",
        CancellationToken ct = default)
    {
        var uri = new Uri($"{AccountLoginGetUrl}?email={login}&password={password}", UriKind.Relative);
        using var authResponse = await client.GetAsync(uri, ct);
        authResponse.EnsureSuccessStatusCode();
        var headers = authResponse.Headers;
        var cookie = headers.FirstOrDefault(e => e.Key == "Set-Cookie").Value.First();
        client.DefaultRequestHeaders.Add("Cookie", new List<string> { cookie });
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

    /// <summary>
    /// Выполнить вызов на ручку для тестов, вернуть результат запроса.
    /// </summary>
    /// <param name="client">Клиент.</param>
    /// <param name="method">HTTP глагол.</param>
    /// <param name="uri">Строка запроса.</param>
    /// <param name="content">Содержимое запроса.</param>
    /// <param name="verify">Проверять ли успешность вызова.</param>
    /// <param name="ct">Токен отмены.</param>
    internal static async Task<HttpResponseMessage> SendTestRequest(this HttpClient client, Request method, Uri uri,
        HttpContent? content = null, bool verify = true, CancellationToken ct = default)
    {
        //HttpMethod httpMethod = HttpMethod.Post;
        var response = method switch
        {
            Request.Get => await client.GetAsync(uri, ct),
            Request.Post => await client.PostAsync(uri, content, ct),
            Request.Put => await client.PutAsync(uri, content, ct),
            Request.Delete => await client.DeleteAsync(uri, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };

        if (verify) response.EnsureSuccessStatusCode();

        return response;
    }
}
