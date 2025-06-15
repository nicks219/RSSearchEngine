using System.Text.Json.Serialization;
using SearchEngine.Data.Configuration;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт ответа ручки с системной информацией.
/// </summary>
/// <param name="Version">Версия сервиса.</param>
/// <param name="DebugBuild">Флаг отладочной сборки.</param>
/// <param name="ReaderContext">Тип бл для чтения.</param>
/// <param name="CreateTablesOnPgMigration">Флаг создания таблиц при миграции Postgres.</param>
public record SystemResponse
(
    [property: JsonPropertyName("Version")] string Version,
    [property: JsonPropertyName("DebugBuild")] bool DebugBuild,
    [property: JsonPropertyName("ReaderContext")] DatabaseType ReaderContext,
    [property: JsonPropertyName("CreateTablesOnPgMigration")] bool CreateTablesOnPgMigration,
    [property: JsonPropertyName("ElectionType")] string ElectionType
);
