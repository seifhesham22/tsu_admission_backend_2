using Admission.Domain.Abstractions;
using Admission.Domain.Applicants;
using Admission.Domain.Catalogue;
using Admission.Domain.Managers;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Admissions;

public sealed class ApplicantAdmission : AggregateRoot
{
    private readonly List<AdmissionProgram> _programs = new();

    public Guid ApplicantId { get; private set; }

    public Applicant Applicant { get; private set; } = null!;

    public AdmissionStatus Status { get; private set; }

    public Guid? ManagerId { get; private set; }

    public Manager? Manager { get; private set; }

    public DateTime LastModifiedUtc { get; private set; }

    public uint Version { get; private set; }

    public IReadOnlyCollection<AdmissionProgram> Programs => _programs.AsReadOnly();

    public bool IsClosed => Status == AdmissionStatus.Closed;

    private ApplicantAdmission()
    {
    }

    private ApplicantAdmission(Guid applicantId)
    {
        ApplicantId = applicantId;
        Status = AdmissionStatus.Created;
        LastModifiedUtc = DateTime.UtcNow;
    }

    public static ApplicantAdmission Open(Guid applicantId)
    {
        if (applicantId == Guid.Empty)
        {
            throw new DomainRuleException("An admission requires a valid applicant identifier.");
        }

        return new ApplicantAdmission(applicantId);
    }

    public void SelectProgram(EducationProgram program, ProgramPriority priority, int maxPrograms)
    {
        ArgumentNullException.ThrowIfNull(program);
        EnsureOpen();

        if (_programs.Count >= maxPrograms)
        {
            throw new DomainRuleException(
                $"An admission cannot contain more than {maxPrograms} programs.");
        }

        if (_programs.Any(x => x.EducationProgramId == program.Id))
        {
            throw new ConflictException("This program has already been selected.");
        }

        _programs.Add(AdmissionProgram.Create(Id, program, priority));
        Touch();
    }

    public void RemoveProgram(Guid admissionProgramId)
    {
        EnsureOpen();

        var program = FindProgram(admissionProgramId);
        _programs.Remove(program);
        Touch();
    }

    public void ChangeProgramPriority(Guid admissionProgramId, ProgramPriority priority)
    {
        EnsureOpen();

        if (!Enum.IsDefined(priority))
        {
            throw new DomainRuleException($"'{priority}' is not a valid priority.");
        }

        var target = FindProgram(admissionProgramId);
        if (target.Priority == priority)
        {
            return;
        }

        var occupant = _programs.FirstOrDefault(x => x.Priority == priority);
        if (occupant is not null)
        {
            occupant.SetPriority(target.Priority);
        }

        target.SetPriority(priority);
        Touch();
    }

    public void ChangeStatus(AdmissionStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainRuleException($"'{status}' is not a valid admission status.");
        }

        if (Status == AdmissionStatus.Closed && status != AdmissionStatus.Closed)
        {
            throw new DomainRuleException("A closed admission cannot be reopened.");
        }

        Status = status;
        Touch();
    }

    public void AssignTo(Manager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        EnsureOpen();

        if (!manager.CanOwnAdmissions)
        {
            throw new DomainRuleException(
                "Only a regular manager can be assigned to an admission.");
        }

        if (ManagerId is not null && ManagerId != manager.Id)
        {
            throw new ConflictException("This admission is already owned by another manager.");
        }

        ManagerId = manager.Id;
        Manager = manager;
        Touch();
    }

    public void ReleaseManager(Manager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        EnsureOpen();

        if (ManagerId is null)
        {
            throw new DomainRuleException("This admission is not owned by any manager.");
        }

        EnsureOwnedBy(manager);

        ManagerId = null;
        Manager = null;
        Touch();
    }

    public void EnsureOwnedBy(Manager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        if (ManagerId != manager.Id)
        {
            throw new ForbiddenException("This admission is not assigned to the current manager.");
        }
    }

    public void EnsureOpen()
    {
        if (IsClosed)
        {
            throw new DomainRuleException("This admission is closed and can no longer be modified.");
        }
    }

    private AdmissionProgram FindProgram(Guid admissionProgramId) =>
        _programs.FirstOrDefault(x => x.Id == admissionProgramId)
        ?? throw NotFoundException.For<AdmissionProgram>(admissionProgramId);

    private void Touch() => LastModifiedUtc = DateTime.UtcNow;
}
