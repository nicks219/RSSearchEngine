using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// Функционал для хранения имен файлов баз sqlite для их последующего удаления
/// </summary>
internal abstract class SqliteFileCleaner
{
    internal static readonly ConcurrentStack<string> Store = new();

    /// <summary>
    /// Запустить отложенную очистку файлов бд (локально, на виндоус)
    /// </summary>
    internal static void ScheduleFileDeletionWindowsOnly()
    {
        if (Docker.IsGitHubAction() || Environment.OSVersion.Platform != PlatformID.Win32NT) return;

        Console.WriteLine($"{nameof(SqliteFileCleaner)} | preparing to start deleting files".ToUpper());
        var args = string.Join(" ", Store.Select(f => $"\"{f}\""));

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c start \"\" cmd /c \"timeout /t 2 & del {args}\"",
            CreateNoWindow = false,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }
}
