using Admission.Application.Admissions.Contracts;
using Admission.Application.Applicants.Contracts;
using Admission.Application.Applicants.Dtos;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Applicants;
using Admission.Domain.Catalogue;
using Shared.Auth;
using Shared.Kernel.Exceptions;

namespace Admission.Application.Applicants.Services;

public sealed class ApplicantDocumentService : IApplicantDocumentService
{
    private readonly IApplicantRepository _applicants;
    private readonly ICatalogueRepository _catalogue;
    private readonly IAdmissionAccessGuard _accessGuard;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ApplicantDocumentService(
        IApplicantRepository applicants,
        ICatalogueRepository catalogue,
        IAdmissionAccessGuard accessGuard,
        ICurrentUserAccessor currentUser,
        IUnitOfWork unitOfWork)
    {
        _applicants = applicants;
        _catalogue = catalogue;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<PassportResponse> GetMyPassportAsync(CancellationToken cancellationToken = default)
    {
        var applicant = await GetCurrentApplicantWithDocumentsAsync(cancellationToken);
        var passport = applicant.Passport
            ?? throw new NotFoundException("No passport record exists for this applicant.");

        return Map(passport);
    }

    public async Task<PassportResponse> AddMyPassportAsync(
        CreatePassportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await GetCurrentApplicantWithDocumentsAsync(cancellationToken);
        await _accessGuard.EnsureCanModifyAsync(applicant.Id, cancellationToken);

        var passport = applicant.AddPassport(
            request.Series,
            request.PlaceOfBirth,
            request.IssuedBy,
            request.IssueDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(passport);
    }

    public async Task<PassportResponse> UpdatePassportAsync(
        Guid applicantId,
        UpdatePassportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await ResolveApplicantAsync(applicantId, cancellationToken);
        await _accessGuard.EnsureCanModifyAsync(applicant.Id, cancellationToken);

        var passport = applicant.Passport
            ?? throw new NotFoundException("No passport record exists for this applicant.");

        passport.Update(request.Series, request.PlaceOfBirth, request.IssuedBy, request.IssueDate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(passport);
    }

    public async Task<EducationDocumentResponse> GetMyEducationDocumentAsync(
        CancellationToken cancellationToken = default)
    {
        var applicant = await GetCurrentApplicantWithDocumentsAsync(cancellationToken);
        var document = applicant.EducationDocument
            ?? throw new NotFoundException("No education document exists for this applicant.");

        return Map(document);
    }

    public async Task<EducationDocumentResponse> AddMyEducationDocumentAsync(
        SaveEducationDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await GetCurrentApplicantWithDocumentsAsync(cancellationToken);
        await _accessGuard.EnsureCanModifyAsync(applicant.Id, cancellationToken);

        var documentType = await GetDocumentTypeAsync(request.DocumentTypeId, cancellationToken);
        var document = applicant.AddEducationDocument(documentType);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(document);
    }

    public async Task<EducationDocumentResponse> UpdateEducationDocumentAsync(
        Guid applicantId,
        SaveEducationDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applicant = await ResolveApplicantAsync(applicantId, cancellationToken);
        await _accessGuard.EnsureCanModifyAsync(applicant.Id, cancellationToken);

        var document = applicant.EducationDocument
            ?? throw new NotFoundException("No education document exists for this applicant.");

        var documentType = await GetDocumentTypeAsync(request.DocumentTypeId, cancellationToken);
        document.ChangeType(documentType);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(document);
    }

    private static PassportResponse Map(Passport passport) =>
        new(
            passport.Id,
            passport.Series,
            passport.PlaceOfBirth,
            passport.IssuedBy,
            passport.IssueDate,
            passport.FileId);

    private static EducationDocumentResponse Map(EducationDocument document) =>
        new(
            document.Id,
            document.DocumentTypeId,
            document.DocumentType?.Name ?? string.Empty,
            document.FileId);

    private async Task<EducationDocumentType> GetDocumentTypeAsync(
        Guid documentTypeId,
        CancellationToken cancellationToken) =>
        await _catalogue.GetDocumentTypeAsync(documentTypeId, cancellationToken)
        ?? throw NotFoundException.For<EducationDocumentType>(documentTypeId);

    private async Task<Applicant> GetCurrentApplicantWithDocumentsAsync(CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();
        return await _applicants.GetWithDocumentsByAuthIdAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException("No applicant profile is linked to the current account.");
    }

    private async Task<Applicant> ResolveApplicantAsync(Guid applicantId, CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();

        if (user.IsApplicant)
        {
            var self = await GetCurrentApplicantWithDocumentsAsync(cancellationToken);
            if (self.Id != applicantId)
            {
                throw new ForbiddenException("An applicant may only modify their own documents.");
            }

            return self;
        }

        return await _applicants.GetWithDocumentsByIdAsync(applicantId, cancellationToken)
            ?? throw NotFoundException.For<Applicant>(applicantId);
    }
}
