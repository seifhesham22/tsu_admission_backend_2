using System.Text.Json.Serialization;

namespace Admission.Application.Sync.Dtos;

public sealed class EducationProgramSyncDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; init; } = string.Empty;

    [JsonPropertyName("educationForm")]
    public string EducationForm { get; init; } = string.Empty;

    [JsonPropertyName("faculty")]
    public FacultySyncDto? Faculty { get; init; }

    [JsonPropertyName("educationLevel")]
    public EducationLevelSyncDto? EducationLevel { get; init; }
}
