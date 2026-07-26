namespace Files.Domain;

public sealed class AdmissionAccess
{
    public Guid ApplicantId { get; private set; }

    public Guid ApplicantAuthId { get; private set; }

    public Guid? AssignedManagerAuthId { get; private set; }

    public AccessStatus Status { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public bool IsOpen => Status == AccessStatus.Open;

    private AdmissionAccess()
    {
    }

    private AdmissionAccess(
        Guid applicantId,
        Guid applicantAuthId,
        Guid? assignedManagerAuthId,
        AccessStatus status,
        DateTime updatedAtUtc)
    {
        ApplicantId = applicantId;
        ApplicantAuthId = applicantAuthId;
        AssignedManagerAuthId = assignedManagerAuthId;
        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static AdmissionAccess Create(
        Guid applicantId,
        Guid applicantAuthId,
        Guid? assignedManagerAuthId,
        AccessStatus status,
        DateTime occurredAtUtc) =>
        new(applicantId, applicantAuthId, assignedManagerAuthId, status, occurredAtUtc);

    public bool TryApply(
        Guid applicantAuthId,
        Guid? assignedManagerAuthId,
        AccessStatus status,
        DateTime occurredAtUtc)
    {
        if (occurredAtUtc <= UpdatedAtUtc)
        {
            return false;
        }

        ApplicantAuthId = applicantAuthId;
        AssignedManagerAuthId = assignedManagerAuthId;
        Status = status;
        UpdatedAtUtc = occurredAtUtc;
        return true;
    }

    public bool AllowsUpload(Guid userAuthId, bool isApplicant, bool isRegularManager, bool isPrivileged)
    {
        if (!IsOpen)
        {
            return false;
        }

        if (isPrivileged)
        {
            return true;
        }

        if (isApplicant)
        {
            return ApplicantAuthId == userAuthId;
        }

        if (isRegularManager)
        {
            return AssignedManagerAuthId == userAuthId;
        }

        return false;
    }

    public bool AllowsRead(Guid userAuthId, bool isApplicant, bool isRegularManager, bool isPrivileged)
    {
        if (isPrivileged)
        {
            return true;
        }

        if (isApplicant)
        {
            return ApplicantAuthId == userAuthId;
        }

        if (isRegularManager)
        {
            return AssignedManagerAuthId == userAuthId;
        }

        return false;
    }
}
