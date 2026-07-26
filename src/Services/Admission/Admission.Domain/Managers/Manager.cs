using Admission.Domain.Abstractions;
using Admission.Domain.Catalogue;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Managers;

public sealed class Manager : AggregateRoot
{
    public Guid AuthId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public ManagerRole Role { get; private set; }

    public Guid? FacultyId { get; private set; }

    public Faculty? Faculty { get; private set; }

    public bool CanOwnAdmissions => Role == ManagerRole.RegularManager;

    public bool IsHeadManager => Role == ManagerRole.HeadManager;

    private Manager()
    {
    }

    private Manager(Guid authId, string email, string fullName, ManagerRole role)
    {
        AuthId = authId;
        Email = email;
        FullName = fullName;
        Role = role;
    }

    public static Manager Create(Guid authId, string email, string? fullName, ManagerRole role)
    {
        if (authId == Guid.Empty)
        {
            throw new DomainRuleException("A manager requires a valid identity identifier.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainRuleException("A manager requires an email address.");
        }

        if (!Enum.IsDefined(role))
        {
            throw new DomainRuleException($"'{role}' is not a valid manager role.");
        }

        return new Manager(
            authId,
            email.Trim(),
            string.IsNullOrWhiteSpace(fullName) ? email.Trim() : fullName.Trim(),
            role);
    }

    public void SyncFromIdentity(string? email, ManagerRole? role)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email.Trim();
        }

        if (role is not null)
        {
            if (!Enum.IsDefined(role.Value))
            {
                throw new DomainRuleException($"'{role}' is not a valid manager role.");
            }

            Role = role.Value;
        }
    }

    public void AssignFaculty(Faculty? faculty)
    {
        Faculty = faculty;
        FacultyId = faculty?.Id;
    }
}
