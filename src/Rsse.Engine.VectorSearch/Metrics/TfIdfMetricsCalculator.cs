using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RsseEngine.Dto;

namespace RsseEngine.Metrics
{
    public sealed class TfIdfMetricsCalculator
    {
        private readonly List<(DocumentId DocumentId, double Metric)> _metrics = new();

        private readonly int _count;

        private double _metricThreshold;

        TfIdfMetricsCalculator(int count = Int32.MaxValue)
        {
            _count = count;
            _metricThreshold = double.MinValue;
        }

        public void AppendMetric(double metric, ExternalDocumentIdWithSize externalDocument)
        {
            if (metric < _metricThreshold)
            {
                return;
            }

            _metrics.Add(new(externalDocument.ExternalDocumentId, metric));

            if (_metrics.Count < _count * 2)
            {
                return;
            }

            _metrics.Sort((left, right) => right.Metric.CompareTo(left.Metric));

            CollectionsMarshal.SetCount(_metrics, _count);

            _metricThreshold = _metrics.Last().Metric;
        }
    }
}
