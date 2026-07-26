using Shared.Kernel.Pagination;

namespace Admission.Application.Catalogue.Dtos;

public sealed class EducationProgramFilter
{
    public IReadOnlyList<Guid>? FacultyIds { get; init; }

    public IReadOnlyList<Guid>? EducationLevelIds { get; init; }

    public IReadOnlyList<string>? Languages { get; init; }

    public IReadOnlyList<string>? EducationForms { get; init; }

    public string? Search { get; init; }

    public PageRequest Page { get; init; } = new();
}
