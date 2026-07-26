using Admission.Application.Managers.Dtos;
using Admission.Application.Managers.Contracts;
using Admission.Application.Managers.Services;
using Shared.Kernel.Pagination;

namespace Admission.Application.Persistence.Contracts;

public interface IManagerQueries
{
    Task<PagedResult<ManagerResponse>> GetManagersAsync(
        PageRequest page,
        CancellationToken cancellationToken = default);
}
