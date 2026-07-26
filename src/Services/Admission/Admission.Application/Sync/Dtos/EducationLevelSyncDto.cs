using System.Text.Json.Serialization;

namespace Admission.Application.Sync.Dtos;

public sealed class EducationLevelSyncDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
