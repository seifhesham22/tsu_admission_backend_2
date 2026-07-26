using Admission.Domain.Admissions;
using Admission.Domain.Managers;
using FluentAssertions;
using Shared.Kernel.Exceptions;
using Xunit;

namespace Admission.Domain.Tests;

public sealed class ApplicantAdmissionTests
{
    private const int MaxPrograms = 3;

    [Fact]
    public void Open_creates_an_admission_in_created_status()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());

        admission.Status.Should().Be(AdmissionStatus.Created);
        admission.IsClosed.Should().BeFalse();
        admission.Programs.Should().BeEmpty();
    }

    [Fact]
    public void SelectProgram_adds_the_program()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var program = TestData.Program();

        admission.SelectProgram(program, ProgramPriority.High, MaxPrograms);

        admission.Programs.Should().ContainSingle()
            .Which.EducationProgramId.Should().Be(program.Id);
    }

    [Fact]
    public void SelectProgram_rejects_duplicates()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var program = TestData.Program();

        admission.SelectProgram(program, ProgramPriority.High, MaxPrograms);

        var act = () => admission.SelectProgram(program, ProgramPriority.Low, MaxPrograms);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void SelectProgram_enforces_the_maximum()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());

        admission.SelectProgram(TestData.Program(), ProgramPriority.High, 2);
        admission.SelectProgram(TestData.Program(), ProgramPriority.Medium, 2);

        var act = () => admission.SelectProgram(TestData.Program(), ProgramPriority.Low, 2);

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*more than 2 programs*");
    }

    [Fact]
    public void A_closed_admission_rejects_every_mutation()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        admission.SelectProgram(TestData.Program(), ProgramPriority.High, MaxPrograms);
        var programId = admission.Programs.Single().Id;

        admission.ChangeStatus(AdmissionStatus.Closed);

        admission.Invoking(x => x.SelectProgram(TestData.Program(), ProgramPriority.Low, MaxPrograms))
            .Should().Throw<DomainRuleException>();

        admission.Invoking(x => x.RemoveProgram(programId))
            .Should().Throw<DomainRuleException>();

        admission.Invoking(x => x.ChangeProgramPriority(programId, ProgramPriority.Low))
            .Should().Throw<DomainRuleException>();

        admission.Invoking(x => x.AssignTo(TestData.RegularManager()))
            .Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void A_closed_admission_cannot_be_reopened()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        admission.ChangeStatus(AdmissionStatus.Closed);

        var act = () => admission.ChangeStatus(AdmissionStatus.UnderReview);

        act.Should().Throw<DomainRuleException>().WithMessage("*cannot be reopened*");
    }

    [Fact]
    public void ChangeProgramPriority_swaps_with_the_current_occupant()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var first = TestData.Program();
        var second = TestData.Program();

        admission.SelectProgram(first, ProgramPriority.High, MaxPrograms);
        admission.SelectProgram(second, ProgramPriority.Low, MaxPrograms);

        var secondEntry = admission.Programs.Single(x => x.EducationProgramId == second.Id);
        admission.ChangeProgramPriority(secondEntry.Id, ProgramPriority.High);

        admission.Programs.Single(x => x.EducationProgramId == second.Id).Priority
            .Should().Be(ProgramPriority.High);

        admission.Programs.Single(x => x.EducationProgramId == first.Id).Priority
            .Should().Be(ProgramPriority.Low);
    }

    [Fact]
    public void AssignTo_rejects_a_head_manager()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());

        var act = () => admission.AssignTo(TestData.HeadManager());

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*regular manager*");
    }

    [Fact]
    public void AssignTo_rejects_a_second_manager()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        admission.AssignTo(TestData.RegularManager());

        var act = () => admission.AssignTo(TestData.RegularManager());

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void AssignTo_is_idempotent_for_the_same_manager()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var manager = TestData.RegularManager();

        admission.AssignTo(manager);
        admission.AssignTo(manager);

        admission.ManagerId.Should().Be(manager.Id);
    }

    [Fact]
    public void EnsureOwnedBy_rejects_a_different_manager()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        admission.AssignTo(TestData.RegularManager());

        var act = () => admission.EnsureOwnedBy(TestData.RegularManager());

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void ReleaseManager_clears_the_assignment()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());
        var manager = TestData.RegularManager();

        admission.AssignTo(manager);
        admission.ReleaseManager(manager);

        admission.ManagerId.Should().BeNull();
    }

    [Fact]
    public void RemoveProgram_rejects_an_unknown_program()
    {
        var admission = ApplicantAdmission.Open(Guid.NewGuid());

        var act = () => admission.RemoveProgram(Guid.NewGuid());

        act.Should().Throw<NotFoundException>();
    }
}
