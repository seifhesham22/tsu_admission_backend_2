namespace Admission.Application.Applicants.Dtos;

public sealed record PassportResponse(
    Guid Id,
    string Series,
    string PlaceOfBirth,
    string IssuedBy,
    DateOnly IssueDate,
    Guid? FileId);
