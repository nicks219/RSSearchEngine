#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

#pragma warning disable CS0162 // Unreachable code detected

namespace SearchEngine.Tools.ClientDevelopmentIntegration;

/// <summary>
/// Подъём и остановка среды разработки JS
/// </summary>
internal static class ClientLauncher
{
    // пользовательские настройки
    private const string ShellCommand = "npm run dev";
    private const bool ShellCommandRunEnabled = true;
    private const string ClientRoot = "../Rsse.Client/ClientApp";
    private const string DevServerUrl = "https://localhost:5173";

    // управление браузером также возможно из launchSettings
    private const bool RunBrowserOnStart = false;
    private const bool KillBrowsersOnStop = true;

    // системные настройки
    private const string ShellProcessName = "rsse.cmd";
    private const string BrowserProcessName = "chrome";
    private const string NodeProcessName = "node";

    private static readonly char SeparatorChar = Path.DirectorySeparatorChar;
    private static readonly string ShellFolder = $"Tools{SeparatorChar}ClientDevelopmentIntegration{SeparatorChar}";
    private static readonly object Lock = new();
    private static volatile bool _initialized;

    /// <summary>
    /// Запустить dev-среду JS в требуемом режиме
    /// </summary>
    /// <param name="args">режимы старта и остановки <b>true - only - false</b></param>
    /// <returns><b>false</b> - выбран отличный от <b>only</b> режим</returns>
    internal static bool Run(string[]? args)
    {
        if (args is not ["--js", _])
        {
            return false;
        }

        var arg = args[1];
        switch (arg)
        {
            case "true":
                Up();
                Console.WriteLine($"info: [{nameof(ClientLauncher)}] starting Vite");
                return false;
            case "only":
                Up();
                Console.WriteLine($"info: [{nameof(ClientLauncher)}] running JS only mode: press any key to stop");
                Console.ReadLine();
                return true;
        }

        return false;
    }

    /// <summary>
    /// Запустить dev server и браузер и подписаться на завершение приложения
    /// </summary>
    private static void Up()
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (isDevelopment == false || _initialized)
        {
            return;
        }

        var pathToShell = Path.Combine(Directory.GetCurrentDirectory(), ShellFolder);

        if (ShellCommandRunEnabled)
        {
            var devServerInitializer = new Process();
            devServerInitializer.StartInfo.FileName = "cmd.exe";
            devServerInitializer.StartInfo.Arguments =
                $"/C cd {ClientRoot} && start {pathToShell}{ShellProcessName} @cmk /k {ShellCommand}";
            devServerInitializer.Start();
            devServerInitializer.WaitForExit();
            devServerInitializer.Close();
        }

        if (RunBrowserOnStart)
        {
            var devClientInitializer = new Process();
            devClientInitializer.StartInfo.FileName = "cmd.exe";
            devClientInitializer.StartInfo.Arguments = $"/C rundll32 url.dll,FileProtocolHandler {DevServerUrl}";
            devClientInitializer.Start();
            devClientInitializer.WaitForExit();
            devClientInitializer.Close();
        }

        // остановить при завершении Main
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Down();
        // остановить из IDE
        Console.CancelKeyPress += (_, _) => Down();

        _initialized = true;
        Console.WriteLine($"info: [{nameof(ClientLauncher)}] started");
    }

    /// <summary>
    /// Остановить dev server и браузер
    /// </summary>
    private static void Down()
    {
        lock (Lock)
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;

            if (ShellCommandRunEnabled)
            {
                // для оставновки dev-сервера Vite будут остановдены процессы node (в данной версии абсолютно все)
                // тк необходимые дочерние процессы node запускаются не только от имени rsse.cmd (но и например от cmd)
                var nodeProcesses = Process.GetProcessesByName(NodeProcessName).ToList();
                var cmdShellNodeProcesses = Process.GetProcessesByName(ShellProcessName).ToList();
                nodeProcesses.ForEach(ps => ps.Kill());
                cmdShellNodeProcesses.ForEach(ps => ps.Kill());
            }

            if (RunBrowserOnStart && KillBrowsersOnStop)
            {
                // остановка браузера: будут закрыты все вкладки и браузеры заданного типа
                var browserProcess = Process.GetProcessesByName(BrowserProcessName)
                    .FirstOrDefault(ps => ps.MainWindowTitle != string.Empty);
                browserProcess?.Kill();
            }

            Console.WriteLine($"info: [{nameof(ClientLauncher)}] stopped");
        }
    }
}
#endif
