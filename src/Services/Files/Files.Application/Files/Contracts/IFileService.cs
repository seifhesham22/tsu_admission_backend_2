using Files.Application.Files.Dtos;
using Files.Domain;

namespace Files.Application.Files.Contracts;

public interface IFileService
{
    Task<StoredFileResponse> UploadAsync(
        Guid applicantId,
        FileKind kind,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFileResponse>> GetForApplicantAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default);

    Task<FileDownload> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        Guid fileId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}
