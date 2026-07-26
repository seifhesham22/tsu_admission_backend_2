using System.ComponentModel.DataAnnotations;
using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Dtos;

public sealed class SelectProgramRequest
{
    [Required]
    public Guid EducationProgramId { get; init; }

    [Required]
    public ProgramPriority Priority { get; init; }
}
