using System.Threading.Tasks;

namespace SearchEngine.Benchmarks;

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
