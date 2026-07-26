using FluentAssertions;
using Files.Domain;
using Xunit;

namespace Files.Domain.Tests;

public sealed class AdmissionAccessTests
{
    private static readonly Guid ApplicantId = Guid.NewGuid();
    private static readonly Guid ApplicantAuthId = Guid.NewGuid();
    private static readonly Guid ManagerAuthId = Guid.NewGuid();

    [Fact]
    public void An_applicant_may_upload_to_their_own_open_admission()
    {
        var access = Open();

        access.AllowsUpload(ApplicantAuthId, isApplicant: true, isRegularManager: false, isPrivileged: false)
            .Should().BeTrue();
    }

    [Fact]
    public void An_applicant_may_not_upload_to_someone_elses_admission()
    {
        var access = Open();

        access.AllowsUpload(Guid.NewGuid(), isApplicant: true, isRegularManager: false, isPrivileged: false)
            .Should().BeFalse();
    }

    [Fact]
    public void Uploads_are_rejected_once_the_admission_is_closed()
    {
        var access = Open();
        access.TryApply(ApplicantAuthId, ManagerAuthId, AccessStatus.Closed, DateTime.UtcNow.AddMinutes(1));

        access.AllowsUpload(ApplicantAuthId, isApplicant: true, isRegularManager: false, isPrivileged: false)
            .Should().BeFalse();
    }

    [Fact]
    public void Reads_remain_allowed_after_the_admission_is_closed()
    {
        var access = Open();
        access.TryApply(ApplicantAuthId, ManagerAuthId, AccessStatus.Closed, DateTime.UtcNow.AddMinutes(1));

        access.AllowsRead(ApplicantAuthId, isApplicant: true, isRegularManager: false, isPrivileged: false)
            .Should().BeTrue();
    }

    [Fact]
    public void Only_the_assigned_regular_manager_may_upload()
    {
        var access = Open();

        access.AllowsUpload(ManagerAuthId, isApplicant: false, isRegularManager: true, isPrivileged: false)
            .Should().BeTrue();

        access.AllowsUpload(Guid.NewGuid(), isApplicant: false, isRegularManager: true, isPrivileged: false)
            .Should().BeFalse();
    }

    [Fact]
    public void A_privileged_user_may_upload_to_an_open_admission()
    {
        var access = Open();

        access.AllowsUpload(Guid.NewGuid(), isApplicant: false, isRegularManager: false, isPrivileged: true)
            .Should().BeTrue();
    }

    [Fact]
    public void A_privileged_user_still_cannot_upload_to_a_closed_admission()
    {
        var access = Open();
        access.TryApply(ApplicantAuthId, ManagerAuthId, AccessStatus.Closed, DateTime.UtcNow.AddMinutes(1));

        access.AllowsUpload(Guid.NewGuid(), isApplicant: false, isRegularManager: false, isPrivileged: true)
            .Should().BeFalse();
    }

    [Fact]
    public void A_stale_event_is_discarded()
    {
        var updatedAt = DateTime.UtcNow;
        var access = AdmissionAccess.Create(
            ApplicantId,
            ApplicantAuthId,
            ManagerAuthId,
            AccessStatus.Closed,
            updatedAt);

        var applied = access.TryApply(
            ApplicantAuthId,
            ManagerAuthId,
            AccessStatus.Open,
            updatedAt.AddMinutes(-5));

        applied.Should().BeFalse();
        access.Status.Should().Be(AccessStatus.Closed);
    }

    [Fact]
    public void A_newer_event_is_applied()
    {
        var access = Open();

        var applied = access.TryApply(
            ApplicantAuthId,
            null,
            AccessStatus.Closed,
            DateTime.UtcNow.AddMinutes(5));

        applied.Should().BeTrue();
        access.Status.Should().Be(AccessStatus.Closed);
        access.AssignedManagerAuthId.Should().BeNull();
    }

    [Fact]
    public void Replaying_the_same_event_is_a_no_op()
    {
        var occurredAt = DateTime.UtcNow;
        var access = AdmissionAccess.Create(
            ApplicantId,
            ApplicantAuthId,
            ManagerAuthId,
            AccessStatus.Open,
            occurredAt);

        access.TryApply(ApplicantAuthId, ManagerAuthId, AccessStatus.Open, occurredAt)
            .Should().BeFalse();
    }

    private static AdmissionAccess Open() =>
        AdmissionAccess.Create(
            ApplicantId,
            ApplicantAuthId,
            ManagerAuthId,
            AccessStatus.Open,
            DateTime.UtcNow);
}
