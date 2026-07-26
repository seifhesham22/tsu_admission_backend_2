namespace Contracts.IntegrationEvents;

public sealed record UserDeleted
{
    public required Guid UserId { get; init; }

    public required DateTime OccurredAtUtc { get; init; }
}
