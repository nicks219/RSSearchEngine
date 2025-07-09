using System.Text.Json.Serialization;
using Rsse.Domain.Data.Configuration;
using Rsse.Domain.Service.ApiModels;
using Rsse.Domain.Service.Configuration;

namespace Rsse.Domain.Service.Contracts;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(DatabaseType))]
[JsonSerializable(typeof(ElectionType))]
[JsonSerializable(typeof(int))]

[JsonSerializable(typeof(CatalogItemResponse))]
[JsonSerializable(typeof(CatalogResponse))]
[JsonSerializable(typeof(CatalogRequest))]
[JsonSerializable(typeof(ComplianceResponse))]
[JsonSerializable(typeof(CredentialsRequest))]
[JsonSerializable(typeof(NoteRequest))]
[JsonSerializable(typeof(NoteResponse))]
[JsonSerializable(typeof(RandomElectionResponse))]
[JsonSerializable(typeof(StringResponse))]
[JsonSerializable(typeof(SystemResponse))]
[JsonSerializable(typeof(UpdateCredentialsRequest))]
public partial class ApiJsonContext : JsonSerializerContext;
