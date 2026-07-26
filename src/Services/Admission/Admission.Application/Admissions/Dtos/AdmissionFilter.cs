using Admission.Domain.Admissions;
using Shared.Kernel.Pagination;

namespace Admission.Application.Admissions.Dtos;

public sealed class AdmissionFilter
{
    public Guid? EducationProgramId { get; init; }

    public IReadOnlyList<Guid>? FacultyIds { get; init; }

    public AdmissionStatus? Status { get; init; }

    public bool OnlyUnassigned { get; init; }

    public bool OnlyMine { get; init; }

    public string? Search { get; init; }

    public PageRequest Page { get; init; } = new();
}
