namespace Contracts.IntegrationEvents;

public sealed record UserEdited
{
    public required Guid UserId { get; init; }

    public string? Email { get; init; }

    public string? Role { get; init; }

    public required DateTime OccurredAtUtc { get; init; }
}
