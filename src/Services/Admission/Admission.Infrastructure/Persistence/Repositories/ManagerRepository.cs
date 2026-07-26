using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Managers;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

public sealed class ManagerRepository : IManagerRepository
{
    private readonly AdmissionDbContext _context;

    public ManagerRepository(AdmissionDbContext context)
    {
        _context = context;
    }

    public Task<Manager?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Managers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Manager?> GetByAuthIdAsync(Guid authId, CancellationToken cancellationToken = default) =>
        _context.Managers.FirstOrDefaultAsync(x => x.AuthId == authId, cancellationToken);

    public async Task<IReadOnlyList<Manager>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Managers
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsForAuthIdAsync(Guid authId, CancellationToken cancellationToken = default) =>
        _context.Managers.AnyAsync(x => x.AuthId == authId, cancellationToken);

    public void Add(Manager manager) => _context.Managers.Add(manager);

    public void Remove(Manager manager) => _context.Managers.Remove(manager);
}
