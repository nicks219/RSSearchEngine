namespace RD.RsseEngine.Service;

/// <summary>
/// Типы функционала подсчета метрик релевантности.
/// </summary>
public enum MetricsCalculatorType
{
    DefaultMetricsCalculator,
    PooledMetricsCalculator,
    NoOpMetricsCalculator
}
