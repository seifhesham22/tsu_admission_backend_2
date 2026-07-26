using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Services;
using Shared.Kernel.Pagination;

namespace Admission.Application.Persistence.Contracts;

public interface IAdmissionQueries
{
    Task<PagedResult<AdmissionSummaryResponse>> SearchAsync(
        AdmissionFilter filter,
        Guid currentManagerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SelectedProgramResponse>> GetSelectedProgramsAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default);
}
