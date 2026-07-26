using Shared.Kernel.Exceptions;

namespace Files.Domain;

public sealed class StoredFile
{
    public const long MaxSizeBytes = 2 * 1024 * 1024;

    public Guid Id { get; private set; }

    public Guid ApplicantId { get; private set; }

    public Guid OwnerAuthId { get; private set; }

    public string OwnerRole { get; private set; } = string.Empty;

    public FileKind Kind { get; private set; }

    public string OriginalFileName { get; private set; } = string.Empty;

    public string StorageKey { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ModifiedAtUtc { get; private set; }

    private StoredFile()
    {
    }

    private StoredFile(
        Guid applicantId,
        Guid ownerAuthId,
        string ownerRole,
        FileKind kind,
        string originalFileName,
        string storageKey,
        long sizeBytes)
    {
        Id = Guid.NewGuid();
        ApplicantId = applicantId;
        OwnerAuthId = ownerAuthId;
        OwnerRole = ownerRole;
        Kind = kind;
        OriginalFileName = originalFileName;
        StorageKey = storageKey;
        SizeBytes = sizeBytes;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static StoredFile Create(
        Guid applicantId,
        Guid ownerAuthId,
        string ownerRole,
        FileKind kind,
        string originalFileName,
        string storageKey,
        long sizeBytes)
    {
        if (applicantId == Guid.Empty)
        {
            throw new ValidationException(nameof(applicantId), "A valid applicant identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ValidationException(nameof(originalFileName), "A file name is required.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ValidationException(nameof(kind), "Unsupported file kind.");
        }

        EnsureSizeIsAllowed(sizeBytes);

        return new StoredFile(
            applicantId,
            ownerAuthId,
            ownerRole,
            kind,
            originalFileName.Trim(),
            storageKey,
            sizeBytes);
    }

    public void Replace(string originalFileName, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ValidationException(nameof(originalFileName), "A file name is required.");
        }

        EnsureSizeIsAllowed(sizeBytes);

        OriginalFileName = originalFileName.Trim();
        SizeBytes = sizeBytes;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    private static void EnsureSizeIsAllowed(long sizeBytes)
    {
        if (sizeBytes <= 0)
        {
            throw new ValidationException(nameof(sizeBytes), "The file is empty.");
        }

        if (sizeBytes > MaxSizeBytes)
        {
            throw new ValidationException(
                nameof(sizeBytes),
                $"The file exceeds the maximum size of {MaxSizeBytes / (1024 * 1024)} MB.");
        }
    }
}
