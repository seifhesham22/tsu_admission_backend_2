using Files.Domain;

namespace Files.Application.Files.Dtos;

public sealed record StoredFileResponse(
    Guid Id,
    Guid ApplicantId,
    FileKind Kind,
    string OriginalFileName,
    long SizeBytes,
    DateTime CreatedAtUtc,
    DateTime? ModifiedAtUtc);
