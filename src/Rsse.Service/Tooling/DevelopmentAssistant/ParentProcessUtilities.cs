using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SearchEngine.Tooling.DevelopmentAssistant;

/// <summary>
/// Утилита для определения родительского процесса.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[ExcludeFromCodeCoverage]
[Obsolete("не используется в данной версии сервиса")]
public struct ParentProcessUtilities
{
    // не редактировать, совпадает с PROCESS_BASIC_INFORMATION
    internal IntPtr Reserved1;
    internal IntPtr PebBaseAddress;
    internal IntPtr Reserved2_0;
    internal IntPtr Reserved2_1;
    internal IntPtr UniqueProcessId;
    internal IntPtr InheritedFromUniqueProcessId;

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

    /// <summary>
    /// Получить родительский процесс по идентификатору заданного процесса.
    /// </summary>
    /// <param name="id">Идентификатор заданного процесса.</param>
    /// <returns>Инстанс родительского процесса.</returns>
    public static Process? GetParentProcess(int id)
    {
        var process = Process.GetProcessById(id);
        return GetParentProcess(process.Handle);
    }

    /// <summary>
    /// Получить родительский процесс указанного процесса.
    /// </summary>
    /// <param name="handle">Хендлер заданного процесса.</param>
    /// <returns>Инстанс родительского процесса.</returns>
    private static Process? GetParentProcess(IntPtr handle)
    {
        var pbi = new ParentProcessUtilities();
        var status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);
        if (status != 0)
        {
            throw new Win32Exception(status);
        }

        try
        {
            return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
        }
        catch (ArgumentException)
        {
            // родительский процесс не найден:
            return null;
        }
    }
}
