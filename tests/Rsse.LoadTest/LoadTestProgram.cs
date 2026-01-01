using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using Rsse.Domain.Service.ApiModels;

namespace Rsse.LoadTest;

/// <summary>
/// Сценарии нагрузочного тестирования для сервиса.
/// </summary>
public static class LoadTestProgram
{
    private static readonly HttpClient Client = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });
    private static CancellationToken Token => CancellationToken.None;

    public static void Main(string[] args)
    {
        Console.WriteLine($"{nameof(LoadTestProgram)} started | step {args[0]}");
        Func<IScenarioContext, Task<IResponse>> run = args[0] switch
        {
            "election" => ElectionStepFunc,
            _ => throw new InvalidOperationException($"Unknown step: '{args[0]}'")
        };

        var scenario = Scenario.Create($"{args[0]}_scenario",
                async context => await run(context).ConfigureAwait(false))
            .WithWarmUpDuration(TimeSpan.FromSeconds(1))
            .WithLoadSimulations(
                // Simulation.KeepConstant(100, TimeSpan.FromSeconds(15))
                Simulation.KeepConstant(6, TimeSpan.FromSeconds(15))
            );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    // выбор заметки
    private static async Task<IResponse> ElectionStepFunc(IScenarioContext _)
    {
        var uri = new Uri("http://localhost:5000/v6/election/note?id=1");
        var content = new NoteRequest();
        using var requestContent =
            new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = requestContent;
        using var response = await Client.SendAsync(request, Token);

        return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
    }

}
