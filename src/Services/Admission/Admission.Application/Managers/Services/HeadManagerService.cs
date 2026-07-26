using Admission.Application.Admissions.Contracts;
using Admission.Application.Managers.Contracts;
using Admission.Application.Managers.Dtos;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Admission.Domain.Applicants;
using Admission.Domain.Managers;
using Shared.Kernel.Exceptions;
using Shared.Kernel.Pagination;

namespace Admission.Application.Managers.Services;

public sealed class HeadManagerService : IHeadManagerService
{
    private readonly IAdmissionRepository _admissions;
    private readonly IApplicantRepository _applicants;
    private readonly IManagerRepository _managers;
    private readonly IManagerQueries _queries;
    private readonly IAdmissionEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public HeadManagerService(
        IAdmissionRepository admissions,
        IApplicantRepository applicants,
        IManagerRepository managers,
        IManagerQueries queries,
        IAdmissionEventPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _admissions = admissions;
        _applicants = applicants;
        _managers = managers;
        _queries = queries;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public Task<PagedResult<ManagerResponse>> GetManagersAsync(
        PageRequest page,
        CancellationToken cancellationToken = default) =>
        _queries.GetManagersAsync(page, cancellationToken);

    public async Task AssignManagerAsync(
        Guid admissionId,
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        var admission = await _admissions.GetByIdAsync(admissionId, cancellationToken)
            ?? throw NotFoundException.For<ApplicantAdmission>(admissionId);

        var manager = await _managers.GetByIdAsync(managerId, cancellationToken)
            ?? throw NotFoundException.For<Manager>(managerId);

        admission.AssignTo(manager);

        var applicant = await _applicants.GetByIdAsync(admission.ApplicantId, cancellationToken)
            ?? throw NotFoundException.For<Applicant>(admission.ApplicantId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAccessChangedAsync(
            admission,
            applicant.AuthId,
            manager.AuthId,
            cancellationToken);

        await _publisher.PublishEmailAsync(
            manager.Email,
            "New admission assigned",
            $"Hello {manager.FullName}, the admission of {applicant.FullName} has been assigned to you.",
            cancellationToken);

        await _publisher.PublishEmailAsync(
            applicant.Email,
            "A manager is now handling your admission",
            $"Hello {applicant.FullName}, {manager.FullName} is now handling your admission.",
            cancellationToken);
    }
}
