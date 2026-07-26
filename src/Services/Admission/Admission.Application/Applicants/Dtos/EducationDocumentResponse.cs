namespace Admission.Application.Applicants.Dtos;

public sealed record EducationDocumentResponse(
    Guid Id,
    Guid DocumentTypeId,
    string DocumentTypeName,
    Guid? FileId);
