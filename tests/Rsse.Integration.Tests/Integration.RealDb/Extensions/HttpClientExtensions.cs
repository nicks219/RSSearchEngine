using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Rsse.Domain.Service.Configuration.RouteConstants;

namespace Rsse.Tests.Integration.RealDb.Extensions;

/// <summary>
/// Расширения для запросов на различные ручки сервиса.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Попытаться авторизоваться в сервисе, прикрепить куки к заголовкам в случае успеха.
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
}
