using Admission.Domain.Catalogue;

namespace Admission.Domain.Applicants;

public sealed class EducationDocument : ApplicantDocument
{
    public Guid DocumentTypeId { get; private set; }

    public EducationDocumentType DocumentType { get; private set; } = null!;

    private EducationDocument()
    {
    }

    private EducationDocument(Guid applicantId, Guid documentTypeId)
        : base(applicantId)
    {
        DocumentTypeId = documentTypeId;
    }

    internal static EducationDocument Create(Guid applicantId, EducationDocumentType documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        return new EducationDocument(applicantId, documentType.Id)
        {
            DocumentType = documentType
        };
    }

    public void ChangeType(EducationDocumentType documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        DocumentTypeId = documentType.Id;
        DocumentType = documentType;
        LastModifiedUtc = DateTime.UtcNow;
    }
}
