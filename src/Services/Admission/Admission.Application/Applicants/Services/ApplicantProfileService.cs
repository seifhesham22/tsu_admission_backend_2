using Admission.Application.Admissions.Contracts;
using Admission.Application.Applicants.Contracts;
using Admission.Application.Applicants.Dtos;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Applicants;
using Shared.Auth;
using Shared.Kernel.Exceptions;

namespace Admission.Application.Applicants.Services;

public sealed class ApplicantProfileService : IApplicantProfileService
{
    private readonly IApplicantRepository _applicants;
    private readonly IAdmissionRepository _admissions;
    private readonly IManagerRepository _managers;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ApplicantProfileService(
        IApplicantRepository applicants,
        IAdmissionRepository admissions,
        IManagerRepository managers,
        ICurrentUserAccessor currentUser,
        IUnitOfWork unitOfWork)
    {
        _applicants = applicants;
        _admissions = admissions;
        _managers = managers;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplicantProfileResponse> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var applicant = await GetCurrentApplicantAsync(cancellationToken);
        return Map(applicant);
    }

    public async Task<ApplicantProfileResponse> UpdateMyProfileAsync(
        UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await GetCurrentApplicantAsync(cancellationToken);
        await EnsureAdmissionEditableAsync(applicant.Id, cancellationToken);

        Apply(applicant, request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(applicant);
    }

    public async Task<ApplicantProfileResponse> UpdateProfileAsync(
        Guid applicantId,
        UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await _applicants.GetByIdAsync(applicantId, cancellationToken)
            ?? throw NotFoundException.For<Applicant>(applicantId);

        await EnsureManagerMayEditAsync(applicantId, cancellationToken);

        Apply(applicant, request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(applicant);
    }

    public async Task<FullApplicantResponse> GetFullProfileAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default)
    {
        var applicant = await _applicants.GetFullProfileAsync(applicantId, cancellationToken)
            ?? throw NotFoundException.For<Applicant>(applicantId);

        var passport = applicant.Passport is null
            ? null
            : new PassportResponse(
                applicant.Passport.Id,
                applicant.Passport.Series,
                applicant.Passport.PlaceOfBirth,
                applicant.Passport.IssuedBy,
                applicant.Passport.IssueDate,
                applicant.Passport.FileId);

        var educationDocument = applicant.EducationDocument is null
            ? null
            : new EducationDocumentResponse(
                applicant.EducationDocument.Id,
                applicant.EducationDocument.DocumentTypeId,
                applicant.EducationDocument.DocumentType?.Name ?? string.Empty,
                applicant.EducationDocument.FileId);

        var admissions = applicant.Admissions
            .Select(admission => new AdmissionSummary(
                admission.Id,
                admission.Status,
                admission.ManagerId,
                admission.Manager?.FullName,
                admission.Programs
                    .OrderBy(program => program.Priority)
                    .Select(program => new AdmissionProgramSummary(
                        program.Id,
                        program.EducationProgramId,
                        program.EducationProgram?.Name ?? string.Empty,
                        program.Priority))
                    .ToList()))
            .ToList();

        return new FullApplicantResponse(
            applicant.Id,
            applicant.FullName,
            applicant.Email,
            applicant.BirthDate,
            applicant.Gender,
            applicant.Citizenship,
            applicant.PhoneNumber,
            applicant.LastModifiedUtc,
            passport,
            educationDocument,
            admissions);
    }

    private static void Apply(Applicant applicant, UpdateApplicantProfileRequest request) =>
        applicant.UpdateProfile(
            request.FullName,
            request.Email,
            request.BirthDate,
            request.Gender,
            request.Citizenship,
            request.PhoneNumber);

    private static ApplicantProfileResponse Map(Applicant applicant) =>
        new(
            applicant.Id,
            applicant.FullName,
            applicant.Email,
            applicant.BirthDate,
            applicant.Gender,
            applicant.Citizenship,
            applicant.PhoneNumber,
            applicant.LastModifiedUtc);

    private async Task<Applicant> GetCurrentApplicantAsync(CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();
        return await _applicants.GetByAuthIdAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException("No applicant profile is linked to the current account.");
    }

    private async Task EnsureAdmissionEditableAsync(Guid applicantId, CancellationToken cancellationToken)
    {
        var admission = await _admissions.GetByApplicantIdAsync(applicantId, cancellationToken);
        admission?.EnsureOpen();
    }

    private async Task EnsureManagerMayEditAsync(Guid applicantId, CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();
        if (user.IsAdmin || user.IsHeadManager)
        {
            return;
        }

        var admission = await _admissions.GetByApplicantIdAsync(applicantId, cancellationToken)
            ?? throw new NotFoundException("This applicant has no admission.");

        admission.EnsureOpen();

        var manager = await _managers.GetByAuthIdAsync(user.Id, cancellationToken)
            ?? throw new ForbiddenException("The current account is not a manager.");

        admission.EnsureOwnedBy(manager);
    }
}
