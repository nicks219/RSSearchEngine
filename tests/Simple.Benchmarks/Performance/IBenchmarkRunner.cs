using System.Threading.Tasks;

namespace SimpleEngine.Benchmarks.Performance;

/// <summary>
/// Контракт компонента с бенчмарками.
/// </summary>
public interface IBenchmarkRunner
{
    /// <summary>
    /// Инициализировать тестовые данные для профилируемого компонента.
    /// </summary>
    public Task Initialize();

    /// <summary>
    /// Запустить бенчмарк.
    /// </summary>
    public void RunBenchmark();
}
