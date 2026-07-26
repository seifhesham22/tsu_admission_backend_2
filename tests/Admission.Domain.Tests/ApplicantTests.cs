using Admission.Domain.Applicants;
using Admission.Domain.Catalogue;
using FluentAssertions;
using Shared.Kernel.Exceptions;
using Xunit;

namespace Admission.Domain.Tests;

public sealed class ApplicantTests
{
    [Fact]
    public void Register_requires_an_email()
    {
        var act = () => Applicant.Register(Guid.NewGuid(), string.Empty, "Someone");

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Register_falls_back_to_the_email_when_no_name_is_supplied()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", null);

        applicant.FullName.Should().Be("person@example.com");
    }

    [Fact]
    public void UpdateProfile_only_changes_supplied_fields()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", "Original Name");

        applicant.UpdateProfile(
            fullName: null,
            email: null,
            birthDate: new DateOnly(2000, 1, 1),
            gender: Gender.Female,
            citizenship: "Egyptian",
            phoneNumber: null);

        applicant.FullName.Should().Be("Original Name");
        applicant.Email.Should().Be("person@example.com");
        applicant.BirthDate.Should().Be(new DateOnly(2000, 1, 1));
        applicant.Gender.Should().Be(Gender.Female);
        applicant.Citizenship.Should().Be("Egyptian");
    }

    [Fact]
    public void UpdateProfile_rejects_a_future_birth_date()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", "Name");
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var act = () => applicant.UpdateProfile(null, null, future, null, null, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void UpdateProfile_advances_the_last_modified_timestamp()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", "Name");
        var before = applicant.LastModifiedUtc;

        applicant.UpdateProfile(null, null, null, null, "Egyptian", null);

        applicant.LastModifiedUtc.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void AddPassport_rejects_a_second_passport()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", "Name");
        applicant.AddPassport("AB123", "Cairo", "Authority", new DateOnly(2020, 1, 1));

        var act = () => applicant.AddPassport("CD456", "Cairo", "Authority", new DateOnly(2021, 1, 1));

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void AddEducationDocument_rejects_a_second_document()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", "Name");
        var type = EducationDocumentType.Create(Guid.NewGuid(), "Bachelor diploma");

        applicant.AddEducationDocument(type);

        var act = () => applicant.AddEducationDocument(type);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ChangeType_updates_both_the_type_id_and_the_navigation()
    {
        var applicant = Applicant.Register(Guid.NewGuid(), "person@example.com", "Name");
        var original = EducationDocumentType.Create(Guid.NewGuid(), "School certificate");
        var replacement = EducationDocumentType.Create(Guid.NewGuid(), "Bachelor diploma");

        var document = applicant.AddEducationDocument(original);
        document.ChangeType(replacement);

        document.DocumentTypeId.Should().Be(replacement.Id);
        document.DocumentType.Name.Should().Be("Bachelor diploma");
    }
}
