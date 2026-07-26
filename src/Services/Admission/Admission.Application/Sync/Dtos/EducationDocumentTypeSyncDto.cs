using System.Text.Json.Serialization;

namespace Admission.Application.Sync.Dtos;

public sealed class EducationDocumentTypeSyncDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("educationLevel")]
    public EducationLevelSyncDto? EducationLevel { get; init; }

    [JsonPropertyName("nextEducationLevels")]
    public IReadOnlyList<EducationLevelSyncDto> NextEducationLevels { get; init; } =
        Array.Empty<EducationLevelSyncDto>();
}
