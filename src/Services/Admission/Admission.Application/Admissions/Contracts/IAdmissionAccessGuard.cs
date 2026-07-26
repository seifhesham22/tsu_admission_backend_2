using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Contracts;

public interface IAdmissionAccessGuard
{
    Task<ApplicantAdmission> EnsureCanModifyAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default);

    Task<ApplicantAdmission> EnsureCanViewAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default);
}
