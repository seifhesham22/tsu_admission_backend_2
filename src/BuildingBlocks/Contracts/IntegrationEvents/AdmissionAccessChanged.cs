namespace Contracts.IntegrationEvents;

public sealed record AdmissionAccessChanged
{
    public required Guid ApplicantId { get; init; }

    public required Guid ApplicantAuthId { get; init; }

    public Guid? AssignedManagerAuthId { get; init; }

    public required AdmissionAccessStatus Status { get; init; }

    public required DateTime OccurredAtUtc { get; init; }
}
