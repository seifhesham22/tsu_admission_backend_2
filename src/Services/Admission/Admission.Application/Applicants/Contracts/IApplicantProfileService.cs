using Admission.Application.Applicants.Dtos;
namespace Admission.Application.Applicants.Contracts;

public interface IApplicantProfileService
{
    Task<ApplicantProfileResponse> GetMyProfileAsync(CancellationToken cancellationToken = default);

    Task<ApplicantProfileResponse> UpdateMyProfileAsync(
        UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ApplicantProfileResponse> UpdateProfileAsync(
        Guid applicantId,
        UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<FullApplicantResponse> GetFullProfileAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default);
}
