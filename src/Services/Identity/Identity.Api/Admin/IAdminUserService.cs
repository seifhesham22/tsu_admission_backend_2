using Identity.Api.Admin.Dtos;
using Shared.Kernel.Pagination;

namespace Identity.Api.Admin;

public interface IAdminUserService
{
    Task<StaffUserResponse> CreateAsync(
        CreateStaffUserRequest request,
        string role,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid userId,
        UpdateStaffUserRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<StaffUserResponse>> GetStaffAsync(
        PageRequest page,
        CancellationToken cancellationToken = default);

    IReadOnlyList<string> GetAssignableRoles();
}
