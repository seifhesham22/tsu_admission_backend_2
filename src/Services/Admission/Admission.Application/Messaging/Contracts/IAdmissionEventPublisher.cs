using Admission.Domain.Admissions;

namespace Admission.Application.Messaging.Contracts;

public interface IAdmissionEventPublisher
{
    Task PublishAccessChangedAsync(
        ApplicantAdmission admission,
        Guid applicantAuthId,
        Guid? assignedManagerAuthId,
        CancellationToken cancellationToken = default);

    Task PublishEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
