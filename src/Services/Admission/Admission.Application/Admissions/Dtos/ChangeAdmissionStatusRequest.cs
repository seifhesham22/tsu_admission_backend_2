using System.ComponentModel.DataAnnotations;
using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Dtos;

public sealed class ChangeAdmissionStatusRequest
{
    [Required]
    public AdmissionStatus Status { get; init; }
}
