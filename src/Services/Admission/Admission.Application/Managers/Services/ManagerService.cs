using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Services;
using Admission.Application.Managers.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Admission.Domain.Applicants;
using Admission.Domain.Managers;
using Shared.Auth;
using Shared.Kernel.Exceptions;
using Shared.Kernel.Pagination;

namespace Admission.Application.Managers.Services;

public sealed class ManagerWorkloadService : IManagerWorkloadService
{
    private readonly IAdmissionRepository _admissions;
    private readonly IApplicantRepository _applicants;
    private readonly IManagerRepository _managers;
    private readonly IAdmissionQueries _queries;
    private readonly IAdmissionEventPublisher _publisher;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ManagerWorkloadService(
        IAdmissionRepository admissions,
        IApplicantRepository applicants,
        IManagerRepository managers,
        IAdmissionQueries queries,
        IAdmissionEventPublisher publisher,
        ICurrentUserAccessor currentUser,
        IUnitOfWork unitOfWork)
    {
        _admissions = admissions;
        _applicants = applicants;
        _managers = managers;
        _queries = queries;
        _publisher = publisher;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AdmissionSummaryResponse>> SearchAdmissionsAsync(
        AdmissionFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var manager = await GetCurrentManagerAsync(cancellationToken);
        return await _queries.SearchAsync(filter, manager.Id, cancellationToken);
    }

    public async Task TakeOwnershipAsync(Guid admissionId, CancellationToken cancellationToken = default)
    {
        var manager = await GetCurrentManagerAsync(cancellationToken);

        var admission = await _admissions.GetByIdAsync(admissionId, cancellationToken)
            ?? throw NotFoundException.For<ApplicantAdmission>(admissionId);

        admission.AssignTo(manager);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishAccessAsync(admission, manager.AuthId, cancellationToken);
    }

    public async Task ReleaseOwnershipAsync(Guid admissionId, CancellationToken cancellationToken = default)
    {
        var manager = await GetCurrentManagerAsync(cancellationToken);

        var admission = await _admissions.GetByIdAsync(admissionId, cancellationToken)
            ?? throw NotFoundException.For<ApplicantAdmission>(admissionId);

        admission.ReleaseManager(manager);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishAccessAsync(admission, assignedManagerAuthId: null, cancellationToken);
    }

    public async Task ChangeStatusAsync(
        Guid admissionId,
        AdmissionStatus status,
        CancellationToken cancellationToken = default)
    {
        var manager = await GetCurrentManagerAsync(cancellationToken);

        var admission = await _admissions.GetByIdAsync(admissionId, cancellationToken)
            ?? throw NotFoundException.For<ApplicantAdmission>(admissionId);

        if (!manager.IsHeadManager)
        {
            admission.EnsureOwnedBy(manager);
        }

        admission.ChangeStatus(status);

        var applicant = await _applicants.GetByIdAsync(admission.ApplicantId, cancellationToken)
            ?? throw NotFoundException.For<Applicant>(admission.ApplicantId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAccessChangedAsync(
            admission,
            applicant.AuthId,
            manager.Id == admission.ManagerId ? manager.AuthId : null,
            cancellationToken);

        await _publisher.PublishEmailAsync(
            applicant.Email,
            "Admission status updated",
            $"Hello {applicant.FullName}, your admission status is now {admission.Status}.",
            cancellationToken);
    }

    private async Task PublishAccessAsync(
        ApplicantAdmission admission,
        Guid? assignedManagerAuthId,
        CancellationToken cancellationToken)
    {
        var applicant = await _applicants.GetByIdAsync(admission.ApplicantId, cancellationToken)
            ?? throw NotFoundException.For<Applicant>(admission.ApplicantId);

        await _publisher.PublishAccessChangedAsync(
            admission,
            applicant.AuthId,
            assignedManagerAuthId,
            cancellationToken);
    }

    private async Task<Manager> GetCurrentManagerAsync(CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();
        return await _managers.GetByAuthIdAsync(user.Id, cancellationToken)
            ?? throw new ForbiddenException("The current account is not registered as a manager.");
    }
}
