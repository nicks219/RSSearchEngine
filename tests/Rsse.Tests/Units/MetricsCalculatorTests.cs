using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RsseEngine.Dto;
using RsseEngine.Service;

namespace Rsse.Tests.Units;

[TestClass]
public class MetricsCalculatorTests
{
    public static IEnumerable<object[]> TestData =>
    [
        // 1. обе метрики пустые
        [
            new List<KeyValuePair<DocumentId, double>>(),
            new List<KeyValuePair<DocumentId, double>>(),
            new List<KeyValuePair<DocumentId, double>>()
        ],
        // 2. метрики только в extended
        [
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 1D) },
            new List<KeyValuePair<DocumentId, double>>(),
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 1D) }
        ],
        // 3. метрики только в reduced
        [
            new List<KeyValuePair<DocumentId, double>>(),
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 1D) },
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 1D) }
        ],
        // 4. совпадающее по ключам перекрытие метрик в extended и reduced
        [
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 1D), new(new DocumentId(0), 2D) },
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 0.5D), new(new DocumentId(0), 0.6D) },
            new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(0), 1D), new(new DocumentId(0), 2D) }
        ]
    ];

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void MetricsCalculator_Tests_Simple(
        List<KeyValuePair<DocumentId, double>> complianceMetricsExtended,
        List<KeyValuePair<DocumentId, double>> complianceMetricsReduced,
        List<KeyValuePair<DocumentId, double>> complianceMetricsResult
    )
    {
        // arrange
        var calculator = new MetricsCalculator();
        calculator.ComplianceMetricsExtended.AddRange(complianceMetricsExtended);
        calculator.ComplianceMetricsReduced.AddRange(complianceMetricsReduced);

        // act
        var result = calculator.ComplianceMetrics;

        // assert
        result
            .Should()
            .BeEquivalentTo(complianceMetricsResult);
    }

    [TestMethod]
    public void MetricsCalculator_Tests_0()
    {
        // arrange
        var complianceMetricsExtended = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 1D) };
        var complianceMetricsReduced = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 0.5D), new(new DocumentId(2), 0.6D) };
        var complianceMetricsResult = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 1D), new(new DocumentId(2), 0.6D) };

        var calculator = new MetricsCalculator();
        calculator.ComplianceMetricsExtended.AddRange(complianceMetricsExtended);
        calculator.ComplianceMetricsReduced.AddRange(complianceMetricsReduced);

        // act
        var result = calculator.ComplianceMetrics;

        // assert
        result
            .Should()
            .BeEquivalentTo(complianceMetricsResult);
    }

    [TestMethod]
    public void MetricsCalculator_Tests_1()
    {
        // arrange
        var complianceMetricsExtended = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 1D), new(new DocumentId(2), 2D) };
        var complianceMetricsReduced = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(3), 0.5D), new(new DocumentId(2), 0.6D) };
        var complianceMetricsResult = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 1D), new(new DocumentId(2), 2D), new(new DocumentId(3), 0.5D) };

        var calculator = new MetricsCalculator();
        calculator.ComplianceMetricsExtended.AddRange(complianceMetricsExtended);
        calculator.ComplianceMetricsReduced.AddRange(complianceMetricsReduced);

        // act
        var result = calculator.ComplianceMetrics;

        // assert
        result
            .Should()
            .BeEquivalentTo(complianceMetricsResult);
    }

    // 6
    [TestMethod]
    public void MetricsCalculator_Tests_2()
    {
        // arrange
        var complianceMetricsExtended = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 1D) };
        var complianceMetricsReduced = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(3), 0.5D), new(new DocumentId(2), 0.6D) };
        var complianceMetricsResult = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(1), 1D), new(new DocumentId(2), 0.6D), new(new DocumentId(3), 0.5D) };

        var calculator = new MetricsCalculator();
        calculator.ComplianceMetricsExtended.AddRange(complianceMetricsExtended);
        calculator.ComplianceMetricsReduced.AddRange(complianceMetricsReduced);

        // act
        var result = calculator.ComplianceMetrics;

        // assert
        result
            .Should()
            .BeEquivalentTo(complianceMetricsResult);
    }

    [TestMethod]
    public void MetricsCalculator_Tests_3_4()
    {
        // arrange
        var complianceMetricsExtended = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(4), 1D) };
        var complianceMetricsReduced = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(3), 0.5D), new(new DocumentId(2), 0.6D) };
        var complianceMetricsResult = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(4), 1D), new(new DocumentId(2), 0.6D), new(new DocumentId(3), 0.5D) };

        var calculator = new MetricsCalculator();
        calculator.ComplianceMetricsExtended.AddRange(complianceMetricsExtended);
        calculator.ComplianceMetricsReduced.AddRange(complianceMetricsReduced);

        // act
        var result = calculator.ComplianceMetrics;

        // assert
        result
            .Should()
            .BeEquivalentTo(complianceMetricsResult);
    }

    [TestMethod]
    public void MetricsCalculator_Tests_6()
    {
        // arrange
        var complianceMetricsExtended = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(3), 1D), new(new DocumentId(2), 1D), new(new DocumentId(1), 0.1D) };
        var complianceMetricsReduced = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(2), 0.5D) };
        var complianceMetricsResult = new List<KeyValuePair<DocumentId, double>> { new(new DocumentId(3), 1D), new(new DocumentId(2), 1D), new(new DocumentId(1), 0.1D) };

        var calculator = new MetricsCalculator();
        calculator.ComplianceMetricsExtended.AddRange(complianceMetricsExtended);
        calculator.ComplianceMetricsReduced.AddRange(complianceMetricsReduced);

        // act
        var result = calculator.ComplianceMetrics;

        // assert
        result
            .Should()
            .BeEquivalentTo(complianceMetricsResult);
    }
}
