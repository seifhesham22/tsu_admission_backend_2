using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;

namespace Admission.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AdmissionDbContext _context;

    public UnitOfWork(AdmissionDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
