using Files.Domain;

namespace Files.Application.Persistence.Contracts;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetByApplicantAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default);

    void Add(StoredFile file);

    void Remove(StoredFile file);
}
