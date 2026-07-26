using Admission.Domain.Managers;

namespace Admission.Application.Managers.Dtos;

public sealed record ManagerResponse(
    Guid Id,
    Guid AuthId,
    string FullName,
    string Email,
    ManagerRole Role,
    Guid? FacultyId,
    string? FacultyName);
