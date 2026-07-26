using Admission.Domain.Abstractions;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Catalogue;

public sealed class EducationDocumentType : Entity
{
    private readonly List<EducationLevel> _nextEducationLevels = new();

    public Guid OneCId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid? CurrentEducationLevelId { get; private set; }

    public EducationLevel? CurrentEducationLevel { get; private set; }

    public IReadOnlyCollection<EducationLevel> NextEducationLevels => _nextEducationLevels.AsReadOnly();

    private EducationDocumentType()
    {
    }

    private EducationDocumentType(Guid oneCId, string name)
    {
        OneCId = oneCId;
        Name = name;
    }

    public static EducationDocumentType Create(Guid oneCId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Document type name is required.");
        }

        return new EducationDocumentType(oneCId, name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Document type name is required.");
        }

        Name = name.Trim();
    }

    public void SetCurrentEducationLevel(EducationLevel? level)
    {
        CurrentEducationLevel = level;
        CurrentEducationLevelId = level?.Id;
    }

    public void ReplaceNextEducationLevels(IEnumerable<EducationLevel> levels)
    {
        ArgumentNullException.ThrowIfNull(levels);

        _nextEducationLevels.Clear();
        _nextEducationLevels.AddRange(levels);
    }

    public bool Allows(EducationLevel level)
    {
        ArgumentNullException.ThrowIfNull(level);

        return CurrentEducationLevelId == level.Id ||
               _nextEducationLevels.Any(x => x.Id == level.Id);
    }
}
