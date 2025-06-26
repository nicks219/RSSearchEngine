using OpenTelemetry.Trace;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Api.Observability;

/// <summary>
/// Компонент, запрещающий сэмплирование на инициализации.
/// </summary>
public class ConditionalSampler(ITokenizerService tokenizer) : Sampler
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
