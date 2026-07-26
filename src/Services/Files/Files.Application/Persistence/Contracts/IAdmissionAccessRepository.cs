using Files.Domain;

namespace Files.Application.Persistence.Contracts;

public interface IAdmissionAccessRepository
{
    Task<AdmissionAccess?> GetAsync(Guid applicantId, CancellationToken cancellationToken = default);

    void Add(AdmissionAccess access);
}
