using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Options;

public sealed class OneCOptions
{
    public const string SectionName = "OneC";

    [Required(AllowEmptyStrings = false)]
    public string BaseUrl { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;

    public string FacultiesPath { get; set; } = "faculties";

    public string EducationLevelsPath { get; set; } = "education_levels";

    public string DocumentTypesPath { get; set; } = "document_types";

    public string ProgramsPath { get; set; } = "programs";

    [Range(1, 500)]
    public int PageSize { get; set; } = 100;
}
