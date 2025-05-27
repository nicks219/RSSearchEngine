using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using SearchEngine.Tests.Integrations.Extensions;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary/> Функционал для работы с контейнерами.
public abstract class Docker
{
    public const int MySqlPort = 3306 + 1;
    public const int PostgresPort = 5432 + 1;
    public const string Localhost = "127.0.0.1";

    public static readonly string? MySqlHostFromGitHub = Environment.GetEnvironmentVariable("MYSQL_HOST");
    public static readonly string? PostgresHostFromGitHub = Environment.GetEnvironmentVariable("POSTGRES_HOST");

    private const string PostgresContainer = "pg_test_17";
    private const string MySqlContainer = " mysql_test_8";
    private const string PostgresVolume = "pg_test";
    private const string MySqlVolume = "mysql_test";
    private const int WaitWarmUpMs = 1000;

    public static bool IsGitHubAction() => Environment.GetEnvironmentVariable("DOTNET_CI") == "true";

    /// <summary/> Остановить и удалить тестовые контейнеры
    public static async Task CleanUpDbContainers(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(Docker)} | cleaning up db containers...");

        var args = $"stop {PostgresContainer}";
        await RunDockerCli(args, cancellationToken);
        args = $"stop {MySqlContainer}";
        await RunDockerCli(args, cancellationToken);
        args = $"rm {PostgresContainer}";
        await RunDockerCli(args, cancellationToken);
        args = $"rm {MySqlContainer}";
        await RunDockerCli(args, cancellationToken);
        args = $"volume rm {PostgresVolume}";
        await RunDockerCli(args, cancellationToken);
        args = $"volume rm {MySqlVolume}";
        await RunDockerCli(args, cancellationToken);
    }

    /// <summary/> Запустить тестовые контейнеры
    public static async Task InitializeDbContainers(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(Docker)} | initializing db containers...");

        // todo: можно попробовать именно механику хелсчеков: --health-cmd="pg_isready -U postgres" --health-interval=1s
        var args = $"run --name {PostgresContainer} -e POSTGRES_PASSWORD=1 -e POSTGRES_USER=1 -e POSTGRES_DB=tagit " +
                   $"-v {PostgresVolume}:/var/lib/postgresql/data -p {PostgresPort}:5432 -d postgres:17.4-alpine3.21";
        await InitializeContainer(args, true, PostgresContainer, "pg_isready", "accepting connections", cancellationToken);

        args = $"run --name {MySqlContainer}  --env=MYSQL_PASSWORD=1  --env=MYSQL_USER=1--env=MYSQL_DATABASE=tagit --env=MYSQL_ROOT_PASSWORD=1 " +
               $"--volume={MySqlVolume}:/var/lib/mysql -p {MySqlPort}:3306 -d mysql:8.0.31-debian";
        await InitializeContainer(args, true, "mysql_test_8", "mysqladmin ping -uroot -p1", "mysqld is alive", cancellationToken);
    }

    /// <summary/> Выполнить команду для docker
    private static Task RunDockerCli(string args, CancellationToken cancellationToken) =>
        InitializeContainer(args, false, "", "", "", cancellationToken);

    /// <summary/> Поднять контейнер и подождать на хелсчеке
    private static async Task InitializeContainer(string args, bool shouldWait, string container,
        string command, string expected, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(InitializeContainer));
        process.Start();
        var processOut = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        Console.WriteLine($"{nameof(InitializeContainer)} | {processOut}");
        var errorMessage = await process.StandardError.ReadToEndAsync(cancellationToken);
        Console.WriteLine(errorMessage);
        await process.WaitForExitAsync(cancellationToken);

        if (!shouldWait) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"exec {container} {command}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var count = 20;
        while (count-- > 0)
        {
            if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(InitializeContainer));
            using var healthcheckProcess = Process.Start(startInfo).EnsureNotNull();
            var output = await healthcheckProcess.StandardOutput.ReadToEndAsync(cancellationToken);
            Console.WriteLine($"{count} sec | {container} | {output}");
            if (!string.IsNullOrEmpty(output) && output.Contains(expected)) return;
            await Task.Delay(WaitWarmUpMs, cancellationToken);
        }

        throw new TestCanceledException($"{nameof(Docker)} | test aborted cause container failed | {container} healthcheck did not complete".ToUpperInvariant());
    }
}
