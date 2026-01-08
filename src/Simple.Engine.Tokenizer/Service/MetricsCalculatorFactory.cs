using System;
using System.Collections.Concurrent;
using SimpleEngine.Contracts;

namespace SimpleEngine.Service;

/// <summary>
/// Фабрика, создающая функционал подсчета метрик релевантности.
/// Использует пул для хранения созданных экземпляров.
/// </summary>
/// <param name="type">Тип функционала подсчета метрик.</param>
public sealed class MetricsCalculatorFactory(MetricsCalculatorType type)
{
    private readonly ConcurrentBag<IMetricsCalculator> _pool = [];

    /// <summary>
    /// Создать либо взять из пула функционал подсчета метрик требуемого типа.
    /// </summary>
    /// <returns>Функционал подсчета метрик релевантности.</returns>
    /// <exception cref="NotSupportedException">Неподдерживаемый тип функционала подсчета метрик.</exception>
    public IMetricsCalculator CreateMetricsCalculator()
    {
        switch (type)
        {
            case MetricsCalculatorType.DefaultMetricsCalculator:
                {
                    return new MetricsCalculator();
                }
            case MetricsCalculatorType.PooledMetricsCalculator:
                {
                    if (!_pool.TryTake(out var metricsCalculator))
                    {
                        metricsCalculator = new MetricsCalculator();
                    }

                    return metricsCalculator;
                }
            case MetricsCalculatorType.NoOpMetricsCalculator:
                {
                    if (!_pool.TryTake(out var metricsCalculator))
                    {
                        metricsCalculator = new NullMetricsCalculator();
                    }

                    return metricsCalculator;
                }
            default:
                {
                    throw new NotSupportedException($"{nameof(MetricsCalculatorType)} {type} not supported.");
                }
        }
    }

    /// <summary>
    /// Вернуть в пул функционал подсчета метрик.
    /// </summary>
    /// <param name="metricsCalculator">Функционал подсчета метрик.</param>
    /// <exception cref="NotSupportedException">Неподдерживаемый тип функционала подсчета метрик.</exception>
    public void ReleaseMetricsCalculator(IMetricsCalculator metricsCalculator)
    {
        switch (type)
        {
            case MetricsCalculatorType.DefaultMetricsCalculator:
                {
                    break;
                }
            case MetricsCalculatorType.PooledMetricsCalculator:
            case MetricsCalculatorType.NoOpMetricsCalculator:
                {
                    metricsCalculator.Clear();
                    _pool.Add(metricsCalculator);
                    break;
                }
            default:
                {
                    throw new NotSupportedException($"{nameof(MetricsCalculatorType)} {type} not supported.");
                }
        }
    }
}
