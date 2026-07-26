using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Services;
using Admission.Domain.Admissions;
using Shared.Kernel.Pagination;

namespace Admission.Application.Managers.Contracts;

public interface IManagerWorkloadService
{
    Task<PagedResult<AdmissionSummaryResponse>> SearchAdmissionsAsync(
        AdmissionFilter filter,
        CancellationToken cancellationToken = default);

    Task TakeOwnershipAsync(Guid admissionId, CancellationToken cancellationToken = default);

    Task ReleaseOwnershipAsync(Guid admissionId, CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(
        Guid admissionId,
        AdmissionStatus status,
        CancellationToken cancellationToken = default);
}
