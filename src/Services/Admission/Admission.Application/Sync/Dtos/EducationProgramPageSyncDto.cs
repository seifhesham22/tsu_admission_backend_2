using System.Text.Json.Serialization;

namespace Admission.Application.Sync.Dtos;

public sealed class EducationProgramPageSyncDto
{
    [JsonPropertyName("programs")]
    public IReadOnlyList<EducationProgramSyncDto> Programs { get; init; } =
        Array.Empty<EducationProgramSyncDto>();

    [JsonPropertyName("pagination")]
    public PaginationSyncDto? Pagination { get; init; }
}
