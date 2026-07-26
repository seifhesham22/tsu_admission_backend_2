using Admission.Domain.Admissions;
using Admission.Domain.Catalogue;
using FluentAssertions;
using Shared.Kernel.Exceptions;
using Xunit;

namespace Admission.Domain.Tests;

public sealed class ProgramSelectionPolicyTests
{
    [Fact]
    public void The_same_level_is_always_compatible()
    {
        var levelId = Guid.NewGuid();
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var first = TestData.Program(levelId);

        admission.SelectProgram(first, ProgramPriority.High, 3);

        var act = () => ProgramSelectionPolicy.EnsureLevelIsCompatible(
            admission,
            TestData.Program(levelId),
            Array.Empty<EducationLevelCombination>());

        act.Should().NotThrow();
    }

    [Fact]
    public void A_different_level_requires_an_allowed_combination()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var first = TestData.Program(Guid.NewGuid());
        admission.SelectProgram(first, ProgramPriority.High, 3);

        var act = () => ProgramSelectionPolicy.EnsureLevelIsCompatible(
            admission,
            TestData.Program(Guid.NewGuid()),
            Array.Empty<EducationLevelCombination>());

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void An_allowed_combination_permits_a_different_level_in_either_direction()
    {
        var levelA = Guid.NewGuid();
        var levelB = Guid.NewGuid();

        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        admission.SelectProgram(TestData.Program(levelB), ProgramPriority.High, 3);

        var combinations = new[] { EducationLevelCombination.Create(levelA, levelB, isAllowed: true) };

        var act = () => ProgramSelectionPolicy.EnsureLevelIsCompatible(
            admission,
            TestData.Program(levelA),
            combinations);

        act.Should().NotThrow();
    }

    [Fact]
    public void An_empty_admission_accepts_any_level()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());

        var act = () => ProgramSelectionPolicy.EnsureLevelIsCompatible(
            admission,
            TestData.Program(),
            Array.Empty<EducationLevelCombination>());

        act.Should().NotThrow();
    }
}
