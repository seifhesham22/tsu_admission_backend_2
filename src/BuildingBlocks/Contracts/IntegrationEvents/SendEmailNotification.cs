namespace Contracts.IntegrationEvents;

public sealed record SendEmailNotification
{
    public required string To { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public required DateTime OccurredAtUtc { get; init; }
}
