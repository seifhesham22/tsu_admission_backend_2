using Admission.Domain.Abstractions;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Catalogue;

public sealed class Faculty : Entity
{
    private readonly List<EducationProgram> _programs = new();

    public Guid OneCId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<EducationProgram> Programs => _programs.AsReadOnly();

    private Faculty()
    {
    }

    private Faculty(Guid oneCId, string name)
    {
        OneCId = oneCId;
        Name = name;
    }

    public static Faculty Create(Guid oneCId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Faculty name is required.");
        }

        return new Faculty(oneCId, name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(nameof(name), "Faculty name is required.");
        }

        Name = name.Trim();
    }
}
