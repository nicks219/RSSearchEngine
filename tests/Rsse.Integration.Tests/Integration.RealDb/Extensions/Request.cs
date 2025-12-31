using System.Net.Http;
using System.Text;
using System.Text.Json;
using Rsse.Domain.Service.Api;
using Rsse.Domain.Service.ApiModels;

namespace Rsse.Tests.Integration.RealDb.Extensions;

/// <summary>
/// Коллекция контента для тестовых запросов.
/// </summary>
internal static class Request
{
    // Corner case: создаёт пустой reduced вектор.
    private static readonly NoteRequest CreateZeroReduced =
        new() { Title = "123", Text = "123", CheckedTags = [1] };

    // Варианты DTO для стандартных тестовых запросов.
    internal static readonly NoteRequest CreateWithBracketsRequest =
        new() { Title = "[1]", Text = "посчитаем до четырёх", CheckedTags = [1] };

    internal static readonly NoteRequest CreateRequest = new()
    { Title = "1", Text = "посчитаем до четырёх", CheckedTags = [1] };

    internal static readonly NoteRequest UpdateWithoutIdRequests =
        new() { Title = "[1]", Text = "раз два три четыре", CheckedTags = [1] };

    private static readonly NoteRequest UpdateRequest = new()
    { Title = "[1]", Text = "раз два три четыре", CheckedTags = [1], NoteIdExchange = 946 };

    private static readonly NoteRequest ReadRequest = new() { CheckedTags = [1] };

    private static readonly CatalogRequest CatalogRequest =
        new(PageNumber: 1, Direction: [(int)CatalogService.Direction.Forward]);

    /// <summary>
    /// Null.
    /// </summary>
    internal static NoteRequest? Null => null;

    /// <summary>
    /// Пустой контент.
    /// </summary>
    internal static StringContent EmptyContent => new(string.Empty);

    // Варианты сериализованного контента для тестовых запросов.
    internal static StringContent CreateContent => new(JsonSerializer.Serialize(CreateWithBracketsRequest),
        Encoding.UTF8, "application/json");

    internal static StringContent CreateZeroReducedContent => new(JsonSerializer.Serialize(CreateZeroReduced),
        Encoding.UTF8, "application/json");

    internal static StringContent UpdateContent =>
        new(JsonSerializer.Serialize(UpdateRequest), Encoding.UTF8, "application/json");

    internal static StringContent ReadContent =>
        new(JsonSerializer.Serialize(ReadRequest), Encoding.UTF8, "application/json");

    internal static StringContent CatalogContent =>
        new(JsonSerializer.Serialize(CatalogRequest), Encoding.UTF8, "application/json");
}
