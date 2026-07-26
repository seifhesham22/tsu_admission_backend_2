using Admission.Domain.Applicants;
using Admission.Domain.Catalogue;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Admissions;

public static class ProgramSelectionPolicy
{
    public static void EnsureDocumentAllowsProgram(
        EducationDocument? educationDocument,
        EducationProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        if (educationDocument?.DocumentType is null)
        {
            return;
        }

        if (!educationDocument.DocumentType.Allows(program.EducationLevel))
        {
            throw new DomainRuleException(
                "The selected program's education level is not permitted by your education document.");
        }
    }

    public static void EnsureLevelIsCompatible(
        ApplicantAdmission admission,
        EducationProgram program,
        IReadOnlyCollection<EducationLevelCombination> combinations)
    {
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(combinations);

        var existing = admission.Programs.FirstOrDefault();
        if (existing?.EducationProgram is null)
        {
            return;
        }

        var existingLevelId = existing.EducationProgram.EducationLevelId;
        if (existingLevelId == program.EducationLevelId)
        {
            return;
        }

        var allowed = combinations.Any(x =>
            x.IsAllowed && x.Matches(existingLevelId, program.EducationLevelId));

        if (!allowed)
        {
            throw new DomainRuleException(
                "The selected program's education level cannot be combined with the already selected programs.");
        }
    }
}
