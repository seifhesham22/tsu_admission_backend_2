using Admission.Application.Admissions.Contracts;
using Admission.Application.Managers.Contracts;
using Admission.Application.Managers.Dtos;
using Admission.Application.Managers.Services;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Pagination;

namespace Admission.Infrastructure.Persistence.Queries;

public sealed class ManagerQueries : IManagerQueries
{
    private readonly AdmissionDbContext _context;

    public ManagerQueries(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ManagerResponse>> GetManagersAsync(
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);

        var query = _context.Managers
            .AsNoTracking()
            .Include(x => x.Faculty);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new ManagerResponse(
                x.Id,
                x.AuthId,
                x.FullName,
                x.Email,
                x.Role,
                x.FacultyId,
                x.Faculty != null ? x.Faculty.Name : null))
            .ToListAsync(cancellationToken);

        return PagedResult<ManagerResponse>.Create(items, total, page);
    }
}
