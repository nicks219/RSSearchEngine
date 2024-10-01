#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS0162 // Unreachable code detected

namespace SearchEngine.Tools.DevelopmentAssistant;

/// <summary>
/// Подъём и остановка среды разработки JS
/// </summary>
internal static class ClientLauncher
{
    // запуск и остановка сервера разработки при старте
    private const bool RunDevServerOnStart = true;
    private const DevServerControl DevServerControl = DevelopmentAssistant.DevServerControl.Manual;
    // настройки проекта для запуска сервера разработки
    private const string ShellCommand = "npm run dev";
    private const string ClientRoot = "../Rsse.Client/ClientApp";
    private const string DevServerUrl = "https://localhost:5173";

    // управление запуском браузера также возможно из launchSettings
    private const bool RunBrowserOnStart = false;
    private const bool KillBrowsersOnStop = true;

    // системные настройки
    private const string ShellProcessName = "rsse.cmd";
    private const string BrowserProcessName = "chrome";
    private const string NodeProcessName = "node";

    private static readonly char SeparatorChar = Path.DirectorySeparatorChar;
    private static readonly string ShellFolder = $"Tools{SeparatorChar}DevelopmentAssistant{SeparatorChar}";
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
                Task.Run(Up);
                Console.WriteLine($"[{nameof(ClientLauncher)}] starting Vite");
                return false;
            case "only":
                Task.Run(Up);
                Console.WriteLine($"[{nameof(ClientLauncher)}] running JS only mode: press any key to stop");
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

        // явный контроль необходим для гарантированного завершения всех дочерних процессов
        if (RunDevServerOnStart)
        {
            Console.WriteLine($"[{nameof(ClientLauncher)}] make sure you have previously installed Node.js and run `npm install`");

            var pathToShell = Path.Combine(Directory.GetCurrentDirectory(), ShellFolder);
            var devServerInitializer = new Process();
            devServerInitializer.StartInfo.FileName = "cmd.exe";
            devServerInitializer.StartInfo.Arguments = DevServerControl == DevServerControl.Manual ?
                $"/C cd {ClientRoot} && start {pathToShell}{ShellProcessName} @cmk /k {ShellCommand}" :
                $"/C cd {ClientRoot} && {ShellCommand}";

            devServerInitializer.Start();

            // Rider в состоянии самостоятельно завершить дочерние процессы с dev-сервером, хотя этому можно помешать
            // В любом случае, при управлении со стороны IDE явно ожидать завершение этого процесса бессмысленно
            if (DevServerControl == DevServerControl.Manual)
            {
                devServerInitializer.WaitForExit();
                devServerInitializer.Close();
            }
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

        // запустить делегат при завершении Main
        AppDomain.CurrentDomain.ProcessExit += (_, _) => TryHackDown();
        // запустить делегат при остановке из IDE
        Console.CancelKeyPress += (_, _) => TryHackDown();

        _initialized = true;
        Console.WriteLine($"[{nameof(ClientLauncher)}] started");
    }

    /// <summary>
    /// Попытаться остановить dev server и браузер.
    /// Костыль, тк при остановке процессов node может быть отказано в доступе.
    /// При этом Rider в состоянии самостоятельно завершить процесс.
    /// </summary>
    private static void TryHackDown()
    {
        Console.WriteLine($"[{nameof(ClientLauncher)}] graceful shutdown initiated");

        lock (Lock)
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;

            //
            if (RunDevServerOnStart && DevServerControl == DevServerControl.Manual)
            {
                // для остановки dev-сервера Vite должны быть остановлены процессы node (в данной версии абсолютно все)
                // тк необходимые дочерние процессы node запускаются не только от имени rsse.cmd (но и например от cmd)
                var nodeProcesses = Process.GetProcessesByName(NodeProcessName).ToList();
                var cmdShellNodeProcesses = Process.GetProcessesByName(ShellProcessName).ToList();
                nodeProcesses.ForEach(ps =>
                {
                    try
                    {
                        ps.Kill();
                    }
                    catch
                    {
                        // ignore
                    }
                });
                cmdShellNodeProcesses.ForEach(ps => ps.Kill());
            }

            if (RunBrowserOnStart && KillBrowsersOnStop)
            {
                // остановка браузера: будут закрыты все вкладки и браузеры заданного типа
                var browserProcess = Process.GetProcessesByName(BrowserProcessName)
                    .FirstOrDefault(ps => ps.MainWindowTitle != string.Empty);
                browserProcess?.Kill();
            }

            Console.WriteLine($"[{nameof(ClientLauncher)}] stopped");
        }
    }
}
#endif
