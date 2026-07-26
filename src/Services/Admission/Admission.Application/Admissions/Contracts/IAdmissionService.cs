using Admission.Application.Admissions.Dtos;
using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Contracts;

public interface IAdmissionService
{
    Task<IReadOnlyList<SelectedProgramResponse>> GetMyProgramsAsync(
        CancellationToken cancellationToken = default);

    Task<Guid> SelectProgramAsync(
        SelectProgramRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveProgramAsync(Guid admissionProgramId, CancellationToken cancellationToken = default);

    Task ChangePriorityAsync(
        Guid admissionProgramId,
        ProgramPriority priority,
        CancellationToken cancellationToken = default);

    Task RemoveProgramForApplicantAsync(
        Guid applicantId,
        Guid admissionProgramId,
        CancellationToken cancellationToken = default);

    Task ChangePriorityForApplicantAsync(
        Guid applicantId,
        Guid admissionProgramId,
        ProgramPriority priority,
        CancellationToken cancellationToken = default);
}
