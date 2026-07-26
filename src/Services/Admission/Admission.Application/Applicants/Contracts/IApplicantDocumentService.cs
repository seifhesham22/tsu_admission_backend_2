using Admission.Application.Applicants.Dtos;
namespace Admission.Application.Applicants.Contracts;

public interface IApplicantDocumentService
{
    Task<PassportResponse> GetMyPassportAsync(CancellationToken cancellationToken = default);

    Task<PassportResponse> AddMyPassportAsync(
        CreatePassportRequest request,
        CancellationToken cancellationToken = default);

    Task<PassportResponse> UpdatePassportAsync(
        Guid applicantId,
        UpdatePassportRequest request,
        CancellationToken cancellationToken = default);

    Task<EducationDocumentResponse> GetMyEducationDocumentAsync(CancellationToken cancellationToken = default);

    Task<EducationDocumentResponse> AddMyEducationDocumentAsync(
        SaveEducationDocumentRequest request,
        CancellationToken cancellationToken = default);

    Task<EducationDocumentResponse> UpdateEducationDocumentAsync(
        Guid applicantId,
        SaveEducationDocumentRequest request,
        CancellationToken cancellationToken = default);
}
