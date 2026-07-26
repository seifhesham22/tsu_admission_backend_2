using Admission.Domain.Admissions;

namespace Admission.Application.Persistence.Contracts;

public interface IAdmissionRepository
{
    Task<ApplicantAdmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApplicantAdmission?> GetByApplicantIdAsync(Guid applicantId, CancellationToken cancellationToken = default);

    Task<ApplicantAdmission?> GetWithProgramsByApplicantIdAsync(Guid applicantId, CancellationToken cancellationToken = default);

    Task<ApplicantAdmission?> GetWithProgramsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(ApplicantAdmission admission);
}
