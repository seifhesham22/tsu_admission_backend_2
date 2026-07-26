using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Options;

public sealed class AdmissionOptions
{
    public const string SectionName = "Admission";

    [Range(1, 10)]
    public int MaxProgramsPerAdmission { get; set; } = 3;
}
