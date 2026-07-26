using System.ComponentModel.DataAnnotations;
using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Dtos;

public sealed class ChangeProgramPriorityRequest
{
    [Required]
    public ProgramPriority Priority { get; init; }
}
