using Admission.Application.Managers.Dtos;
using Shared.Kernel.Pagination;

namespace Admission.Application.Managers.Contracts;

public interface IHeadManagerService
{
    Task<PagedResult<ManagerResponse>> GetManagersAsync(
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task AssignManagerAsync(
        Guid admissionId,
        Guid managerId,
        CancellationToken cancellationToken = default);
}
