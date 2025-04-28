using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary/> Функционал для работы с контейнерами
public abstract class Docker
{
    public const int MySqlPort = 3306 + 1;
    public const int PostgresPort = 5432 + 1;

    public static readonly string? MySqlHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
    public static readonly string? PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");

    private const string PostgresContainer = "pg_test_17";
    private const string MySqlContainer = " mysql_test_8";
    private const string PostgresVolume = "pg_test";
    private const string MySqlVolume = "mysql_test";

    public static bool IsGitHubAction() => Environment.GetEnvironmentVariable("DOTNET_CI") == "true";

    /// <summary/> Остановить и удалить тестовые контейнеры
    public static void CleanUpDbContainers()
    {
        var args = $"stop {PostgresContainer}";
        RunDockerCli(args);
        args = $"stop {MySqlContainer}";
        RunDockerCli(args);
        args = $"rm {PostgresContainer}";
        RunDockerCli(args);
        args = $"rm {MySqlContainer}";
        RunDockerCli(args);
        args = $"volume rm {PostgresVolume}";
        RunDockerCli(args);
        args = $"volume rm {MySqlVolume}";
        RunDockerCli(args);
    }

    /// <summary/> Запустить тестовые контейнеры
    public static void InitializeDbContainers()
    {
        // todo: можно попробовать именно механику хелсчеков: --health-cmd="pg_isready -U postgres" --health-interval=1s
        var args = $"run --name {PostgresContainer} -e POSTGRES_PASSWORD=1 -e POSTGRES_USER=1 -e POSTGRES_DB=tagit " +
                   $"-v {PostgresVolume}:/var/lib/postgresql/data -p {PostgresPort}:5432 -d postgres:17.4-alpine3.21";
        InitializeContainer(args, true, PostgresContainer, "pg_isready", "accepting connections");
        args = $"run --name {MySqlContainer}  --env=MYSQL_PASSWORD=1  --env=MYSQL_USER=1--env=MYSQL_DATABASE=tagit --env=MYSQL_ROOT_PASSWORD=1 " +
               $"--volume={MySqlVolume}:/var/lib/mysql -p {MySqlPort}:3306 -d mysql:8.0.31-debian";
        InitializeContainer(args, true,"mysql_test_8", "mysqladmin ping -uroot -p1", "mysqld is alive");
    }

    /// <summary/> Выполнить команду для docker
    private static void RunDockerCli(string args) => InitializeContainer(args, false, "", "", "");

    /// <summary/> Поднять контейнер и подождать на хелсчеке
    private static void InitializeContainer(string args, bool shouldWait, string container, string command, string expected)
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

        process.Start();
        var processOut = process.StandardOutput.ReadToEnd();
        Console.WriteLine(processOut);
        Console.WriteLine(process.StandardError.ReadToEnd());
        process.WaitForExit();

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
            using var healthcheckProcess = Process.Start(startInfo);
            var output = healthcheckProcess?.StandardOutput.ReadToEnd();
            Console.WriteLine($"{count} sec | {container} | {output}");
            if (output != null && output.Contains(expected)) return;
            Thread.Sleep(1000);
        }

        throw new TestCanceledException($"{container} healthcheck did not complete");
    }
}
