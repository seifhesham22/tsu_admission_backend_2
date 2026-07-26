using System.Text.Json.Serialization;

namespace Admission.Application.Sync.Dtos;

public sealed class PaginationSyncDto
{
    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("current")]
    public int Current { get; init; }
}
