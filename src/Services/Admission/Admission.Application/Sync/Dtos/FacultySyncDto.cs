using System.Text.Json.Serialization;

namespace Admission.Application.Sync.Dtos;

public sealed class FacultySyncDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
