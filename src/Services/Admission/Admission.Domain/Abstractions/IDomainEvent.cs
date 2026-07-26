namespace Admission.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
