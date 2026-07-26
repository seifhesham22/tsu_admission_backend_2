using Admission.Domain.Abstractions;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Catalogue;

public sealed class EducationProgram : Entity
{
    public Guid OneCId { get; private set; }

    public Guid FacultyId { get; private set; }

    public Faculty Faculty { get; private set; } = null!;

    public Guid EducationLevelId { get; private set; }

    public EducationLevel EducationLevel { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string Language { get; private set; } = string.Empty;

    public string EducationForm { get; private set; } = string.Empty;

    private EducationProgram()
    {
    }

    private EducationProgram(
        Guid oneCId,
        Guid facultyId,
        Guid educationLevelId,
        string name,
        string code,
        string language,
        string educationForm)
    {
        OneCId = oneCId;
        FacultyId = facultyId;
        EducationLevelId = educationLevelId;
        Name = name;
        Code = code;
        Language = language;
        EducationForm = educationForm;
    }

    public static EducationProgram Create(
        Guid oneCId,
        Guid facultyId,
        Guid educationLevelId,
        string name,
        string code,
        string language,
        string educationForm)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Program name is required.");
        }

        return new EducationProgram(
            oneCId,
            facultyId,
            educationLevelId,
            name.Trim(),
            code?.Trim() ?? string.Empty,
            language?.Trim() ?? string.Empty,
            educationForm?.Trim() ?? string.Empty);
    }

    public void Update(
        Guid facultyId,
        Guid educationLevelId,
        string name,
        string code,
        string language,
        string educationForm)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Program name is required.");
        }

        FacultyId = facultyId;
        EducationLevelId = educationLevelId;
        Name = name.Trim();
        Code = code?.Trim() ?? string.Empty;
        Language = language?.Trim() ?? string.Empty;
        EducationForm = educationForm?.Trim() ?? string.Empty;
    }
}
