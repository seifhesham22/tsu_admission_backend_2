using Admission.Domain.Managers;

namespace Admission.Application.Persistence.Contracts;

public interface IManagerRepository
{
    Task<Manager?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Manager?> GetByAuthIdAsync(Guid authId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Manager>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsForAuthIdAsync(Guid authId, CancellationToken cancellationToken = default);

    void Add(Manager manager);

    void Remove(Manager manager);
}
