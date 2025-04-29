using Microsoft.Extensions.Configuration;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// Маркерный класс для разграничения типов обобщенной тестовой фабрики
/// </summary>
/// <param name="configuration"></param>
// ReSharper disable once ClassNeverInstantiated.Global
internal class SqliteApiStartup(IConfiguration configuration) : SqliteStartup(configuration);
