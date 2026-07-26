using Admission.Domain.Abstractions;
using Admission.Domain.Catalogue;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Admissions;

public sealed class AdmissionProgram : Entity
{
    public Guid ApplicantAdmissionId { get; private set; }

    public Guid EducationProgramId { get; private set; }

    public EducationProgram EducationProgram { get; private set; } = null!;

    public ProgramPriority Priority { get; private set; }

    private AdmissionProgram()
    {
    }

    private AdmissionProgram(Guid applicantAdmissionId, Guid educationProgramId, ProgramPriority priority)
    {
        ApplicantAdmissionId = applicantAdmissionId;
        EducationProgramId = educationProgramId;
        Priority = priority;
    }

    internal static AdmissionProgram Create(
        Guid applicantAdmissionId,
        EducationProgram program,
        ProgramPriority priority)
    {
        ArgumentNullException.ThrowIfNull(program);

        if (!Enum.IsDefined(priority))
        {
            throw new DomainRuleException($"'{priority}' is not a valid priority.");
        }

        return new AdmissionProgram(applicantAdmissionId, program.Id, priority)
        {
            EducationProgram = program
        };
    }

    internal void SetPriority(ProgramPriority priority) => Priority = priority;
}
