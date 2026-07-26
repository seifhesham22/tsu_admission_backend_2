using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Dtos;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Options;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Admission.Domain.Applicants;
using Admission.Domain.Catalogue;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Kernel.Exceptions;

namespace Admission.Application.Admissions.Services;

public sealed class AdmissionService : IAdmissionService
{
    private readonly IAdmissionRepository _admissions;
    private readonly IApplicantRepository _applicants;
    private readonly IManagerRepository _managers;
    private readonly ICatalogueRepository _catalogue;
    private readonly IAdmissionQueries _queries;
    private readonly IAdmissionAccessGuard _accessGuard;
    private readonly IAdmissionEventPublisher _publisher;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AdmissionOptions _options;

    public AdmissionService(
        IAdmissionRepository admissions,
        IApplicantRepository applicants,
        IManagerRepository managers,
        ICatalogueRepository catalogue,
        IAdmissionQueries queries,
        IAdmissionAccessGuard accessGuard,
        IAdmissionEventPublisher publisher,
        ICurrentUserAccessor currentUser,
        IUnitOfWork unitOfWork,
        IOptions<AdmissionOptions> options)
    {
        _admissions = admissions;
        _applicants = applicants;
        _managers = managers;
        _catalogue = catalogue;
        _queries = queries;
        _accessGuard = accessGuard;
        _publisher = publisher;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<SelectedProgramResponse>> GetMyProgramsAsync(
        CancellationToken cancellationToken = default)
    {
        var applicant = await GetCurrentApplicantAsync(cancellationToken);
        return await _queries.GetSelectedProgramsAsync(applicant.Id, cancellationToken);
    }

    public async Task<Guid> SelectProgramAsync(
        SelectProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await GetCurrentApplicantWithDocumentsAsync(cancellationToken);

        var program = await _catalogue.GetProgramWithLevelAsync(request.EducationProgramId, cancellationToken)
            ?? throw NotFoundException.For<EducationProgram>(request.EducationProgramId);

        var admission = await _admissions.GetWithProgramsByApplicantIdAsync(applicant.Id, cancellationToken);
        if (admission is null)
        {
            admission = ApplicantAdmission.Open(applicant.Id);
            _admissions.Add(admission);
        }

        admission.EnsureOpen();

        ProgramSelectionPolicy.EnsureDocumentAllowsProgram(applicant.EducationDocument, program);

        var combinations = await _catalogue.GetCombinationsAsync(cancellationToken);
        ProgramSelectionPolicy.EnsureLevelIsCompatible(admission, program, combinations);

        admission.SelectProgram(program, request.Priority, _options.MaxProgramsPerAdmission);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishAccessAsync(admission, applicant, cancellationToken);

        return admission.Programs.Single(x => x.EducationProgramId == program.Id).Id;
    }

    public async Task RemoveProgramAsync(Guid admissionProgramId, CancellationToken cancellationToken = default)
    {
        var applicant = await GetCurrentApplicantAsync(cancellationToken);
        await RemoveProgramCoreAsync(applicant.Id, admissionProgramId, cancellationToken);
    }

    public async Task ChangePriorityAsync(
        Guid admissionProgramId,
        ProgramPriority priority,
        CancellationToken cancellationToken = default)
    {
        var applicant = await GetCurrentApplicantAsync(cancellationToken);
        await ChangePriorityCoreAsync(applicant.Id, admissionProgramId, priority, cancellationToken);
    }

    public async Task RemoveProgramForApplicantAsync(
        Guid applicantId,
        Guid admissionProgramId,
        CancellationToken cancellationToken = default)
    {
        await _accessGuard.EnsureCanModifyAsync(applicantId, cancellationToken);
        await RemoveProgramCoreAsync(applicantId, admissionProgramId, cancellationToken);
    }

    public async Task ChangePriorityForApplicantAsync(
        Guid applicantId,
        Guid admissionProgramId,
        ProgramPriority priority,
        CancellationToken cancellationToken = default)
    {
        await _accessGuard.EnsureCanModifyAsync(applicantId, cancellationToken);
        await ChangePriorityCoreAsync(applicantId, admissionProgramId, priority, cancellationToken);
    }

    private async Task RemoveProgramCoreAsync(
        Guid applicantId,
        Guid admissionProgramId,
        CancellationToken cancellationToken)
    {
        var admission = await _admissions.GetWithProgramsByApplicantIdAsync(applicantId, cancellationToken)
            ?? throw new NotFoundException("This applicant has no admission.");

        admission.RemoveProgram(admissionProgramId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ChangePriorityCoreAsync(
        Guid applicantId,
        Guid admissionProgramId,
        ProgramPriority priority,
        CancellationToken cancellationToken)
    {
        var admission = await _admissions.GetWithProgramsByApplicantIdAsync(applicantId, cancellationToken)
            ?? throw new NotFoundException("This applicant has no admission.");

        admission.ChangeProgramPriority(admissionProgramId, priority);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task PublishAccessAsync(
        ApplicantAdmission admission,
        Applicant applicant,
        CancellationToken cancellationToken)
    {
        Guid? managerAuthId = null;
        if (admission.ManagerId is { } managerId)
        {
            var manager = await _managers.GetByIdAsync(managerId, cancellationToken);
            managerAuthId = manager?.AuthId;
        }

        await _publisher.PublishAccessChangedAsync(
            admission,
            applicant.AuthId,
            managerAuthId,
            cancellationToken);
    }

    private async Task<Applicant> GetCurrentApplicantAsync(CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();
        return await _applicants.GetByAuthIdAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException("No applicant profile is linked to the current account.");
    }

    private async Task<Applicant> GetCurrentApplicantWithDocumentsAsync(CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();
        return await _applicants.GetWithDocumentsByAuthIdAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException("No applicant profile is linked to the current account.");
    }
}
