using Admission.Domain.Abstractions;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Applicants;

public abstract class ApplicantDocument : Entity
{
    public Guid ApplicantId { get; private set; }

    public Applicant Applicant { get; private set; } = null!;

    public Guid? FileId { get; private set; }

    public DateTime LastModifiedUtc { get; protected set; } = DateTime.UtcNow;

    protected ApplicantDocument()
    {
    }

    protected ApplicantDocument(Guid applicantId)
    {
        ApplicantId = applicantId;
    }

    public void AttachFile(Guid fileId)
    {
        if (fileId == Guid.Empty)
        {
            throw new ValidationException(nameof(fileId), "A valid file identifier is required.");
        }

        FileId = fileId;
        LastModifiedUtc = DateTime.UtcNow;
    }

    public void DetachFile()
    {
        FileId = null;
        LastModifiedUtc = DateTime.UtcNow;
    }
}
