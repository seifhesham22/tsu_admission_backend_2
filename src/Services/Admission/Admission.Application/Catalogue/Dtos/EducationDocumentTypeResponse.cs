namespace Admission.Application.Catalogue.Dtos;

public sealed record EducationDocumentTypeResponse(Guid Id, string Name, Guid? CurrentEducationLevelId);
