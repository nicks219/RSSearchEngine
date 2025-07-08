using RsseEngine.SearchType;

namespace Rsse.Tests.Common;

/// <summary>
/// Общие данные для тестов.
/// </summary>
public abstract class TestData
{
    /// <summary>
    /// Перечисление extended алгоритмов.
    /// </summary>
    public static readonly List<ExtendedSearchType> ExtendedSearchTypes =
    [ExtendedSearchType.Legacy,
        ExtendedSearchType.GinSimple,
        ExtendedSearchType.GinOptimized, ExtendedSearchType.GinFilter,
        ExtendedSearchType.GinFast, ExtendedSearchType.GinFastFilter];

    /// <summary>
    /// Перечисление reduced алгоритмов.
    /// </summary>
    public static readonly List<ReducedSearchType> ReducedSearchTypes =
    [ReducedSearchType.Legacy,
        ReducedSearchType.GinSimple,
        ReducedSearchType.GinOptimized, ReducedSearchType.GinOptimizedFilter,
        ReducedSearchType.GinFilter,
        ReducedSearchType.GinFast, ReducedSearchType.GinFastFilter];

    /// <summary>
    /// Метрики на запросы к дампу pg_backup_.txtnotes
    /// </summary>
    public static IEnumerable<object?[]> ComplianceTestData =>
    [
        ["чорт з ным зо сталом", """{"res":{"1":0.43478260869565216},"error":null}"""],
        ["чёрт с ними за столом", """{"res":{"1":52.631578947368425},"error":null}"""],
        ["с ними за столом чёрт", """{"res":{"1":4.2105263157894735},"error":null}"""],
        ["преключиться вдруг верный друг", """{"res":{"444":0.35714285714285715,"243":0.02},"error":null}"""],
        ["приключится вдруг верный друг", """{"res":{"444":35.08771929824562},"error":null}"""],
        ["пляшем на", """{"res":{"1":21.05263157894737},"error":null}"""],
        ["ты шла по палубе в молчаний", """{"res":{"10":5.154639175257731},"error":null}"""],
        ["оно шла по палубе в молчаний", """{"res":{"10":0.6818181818181818},"error":null}"""],
        ["123 456 иии", """{"res":null,"error":null}"""],
        ["aa bb cc dd .,/#", """{"res":null,"error":null}"""],
        [" |", """{"res":null,"error":null}"""]
    ];
}
