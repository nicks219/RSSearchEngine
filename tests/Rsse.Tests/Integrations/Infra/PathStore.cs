using System.Collections.Concurrent;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// Хранилище имён файлов sqlite для последующего удаления
/// </summary>
internal abstract class PathStore
{
    internal static readonly ConcurrentStack<string> Store = new();
}
