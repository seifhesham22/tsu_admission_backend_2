using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Shared.Auth;
using Shared.Kernel.Exceptions;

namespace Admission.Application.Admissions.Services;

public sealed class AdmissionAccessGuard : IAdmissionAccessGuard
{
    private readonly IAdmissionRepository _admissions;
    private readonly IApplicantRepository _applicants;
    private readonly IManagerRepository _managers;
    private readonly ICurrentUserAccessor _currentUser;

    public AdmissionAccessGuard(
        IAdmissionRepository admissions,
        IApplicantRepository applicants,
        IManagerRepository managers,
        ICurrentUserAccessor currentUser)
    {
        _admissions = admissions;
        _applicants = applicants;
        _managers = managers;
        _currentUser = currentUser;
    }

    public async Task<ApplicantAdmission> EnsureCanModifyAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default)
    {
        var admission = await AuthorizeAsync(applicantId, cancellationToken);
        admission.EnsureOpen();
        return admission;
    }

    public Task<ApplicantAdmission> EnsureCanViewAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default) =>
        AuthorizeAsync(applicantId, cancellationToken);

    private async Task<ApplicantAdmission> AuthorizeAsync(Guid applicantId, CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();

        var admission = await _admissions.GetByApplicantIdAsync(applicantId, cancellationToken)
            ?? throw new NotFoundException("This applicant has no admission.");

        if (user.IsAdmin || user.IsHeadManager)
        {
            return admission;
        }

        if (user.IsRegularManager)
        {
            var manager = await _managers.GetByAuthIdAsync(user.Id, cancellationToken)
                ?? throw new ForbiddenException("The current account is not a manager.");

            admission.EnsureOwnedBy(manager);
            return admission;
        }

        if (user.IsApplicant)
        {
            var applicant = await _applicants.GetByIdAsync(applicantId, cancellationToken)
                ?? throw NotFoundException.For<ApplicantAdmission>(applicantId);

            if (applicant.AuthId != user.Id)
            {
                throw new ForbiddenException("An applicant may only access their own admission.");
            }

            return admission;
        }

        throw new ForbiddenException($"Role '{user.Role}' cannot access admissions.");
    }
}
