using Admission.Domain.Applicants;

namespace Admission.Application.Persistence.Contracts;

public interface IApplicantRepository
{
    Task<Applicant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Applicant?> GetByAuthIdAsync(Guid authId, CancellationToken cancellationToken = default);

    Task<Applicant?> GetWithDocumentsByAuthIdAsync(Guid authId, CancellationToken cancellationToken = default);

    Task<Applicant?> GetWithDocumentsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Applicant?> GetFullProfileAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForAuthIdAsync(Guid authId, CancellationToken cancellationToken = default);

    void Add(Applicant applicant);
}
