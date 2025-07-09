using OpenTelemetry.Trace;
using Rsse.Domain.Service.Contracts;

namespace Rsse.Api.Observability;

/// <summary>
/// Компонент, запрещающий сэмплирование на инициализации.
/// </summary>
public class ConditionalSampler(ITokenizerApiClient tokenizer) : Sampler
{
    /// <summary>
    /// Запретить сэмплирование Npgsql на инициализации.
    /// </summary>
    /// <param name="samplingParameters"></param>
    /// <returns></returns>
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        if (!tokenizer.IsInitialized() && samplingParameters.Name == "tagit")
        {
            return new SamplingResult(SamplingDecision.Drop);
        }

        return new SamplingResult(SamplingDecision.RecordAndSample);
    }
}
