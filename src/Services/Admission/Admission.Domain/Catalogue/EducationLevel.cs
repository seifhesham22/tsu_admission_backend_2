using Admission.Domain.Abstractions;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Catalogue;

public sealed class EducationLevel : Entity
{
    private readonly List<EducationDocumentType> _applicableDocumentTypes = new();

    public int OneCId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<EducationDocumentType> ApplicableDocumentTypes =>
        _applicableDocumentTypes.AsReadOnly();

    private EducationLevel()
    {
    }

    private EducationLevel(int oneCId, string name)
    {
        OneCId = oneCId;
        Name = name;
    }

    public static EducationLevel Create(int oneCId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Education level name is required.");
        }

        return new EducationLevel(oneCId, name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Education level name is required.");
        }

        Name = name.Trim();
    }
}
