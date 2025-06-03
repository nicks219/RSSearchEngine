using System;
using System.Diagnostics.CodeAnalysis;

namespace SearchEngine.Tests.Integration.FakeDb.Extensions;

/// <summary>
/// Для отключения проверки типа на null в коде
/// </summary>
public static class ThrowableExtensions
{
    [return: NotNull]
    public static T EnsureNotNull<T>([NotNull] this T? obj)
    {
        if (obj == null) throw new NullReferenceException(nameof(EnsureNotNull));
        return obj;
    }
}
