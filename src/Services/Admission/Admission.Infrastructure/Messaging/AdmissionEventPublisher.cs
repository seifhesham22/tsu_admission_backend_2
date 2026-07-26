using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Contracts.IntegrationEvents;
using MassTransit;

namespace Admission.Infrastructure.Messaging;

public sealed class AdmissionEventPublisher : IAdmissionEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public AdmissionEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAccessChangedAsync(
        ApplicantAdmission admission,
        Guid applicantAuthId,
        Guid? assignedManagerAuthId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(admission);

        return _publishEndpoint.Publish(
            new AdmissionAccessChanged
            {
                ApplicantId = admission.ApplicantId,
                ApplicantAuthId = applicantAuthId,
                AssignedManagerAuthId = assignedManagerAuthId,
                Status = admission.IsClosed
                    ? AdmissionAccessStatus.Closed
                    : AdmissionAccessStatus.Open,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }

    public Task PublishEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default) =>
        _publishEndpoint.Publish(
            new SendEmailNotification
            {
                To = to,
                Subject = subject,
                Body = body,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);
}
