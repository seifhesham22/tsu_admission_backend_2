using Files.Application.Files.Contracts;
using Files.Application.Files.Dtos;
using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;
using Files.Domain;
using Shared.Auth;
using Shared.Kernel.Exceptions;

namespace Files.Application.Files.Services;

public sealed class FileService : IFileService
{
    private const string PdfContentType = "application/pdf";
    private static readonly byte[] PdfMagicNumber = { 0x25, 0x50, 0x44, 0x46 };

    private readonly IStoredFileRepository _files;
    private readonly IAdmissionAccessRepository _access;
    private readonly IFileStorage _storage;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public FileService(
        IStoredFileRepository files,
        IAdmissionAccessRepository access,
        IFileStorage storage,
        ICurrentUserAccessor currentUser,
        IUnitOfWork unitOfWork)
    {
        _files = files;
        _access = access;
        _storage = storage;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<StoredFileResponse> UploadAsync(
        Guid applicantId,
        FileKind kind,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var user = _currentUser.Get();
        var access = await GetAccessAsync(applicantId, cancellationToken);

        if (!access.AllowsUpload(user.Id, user.IsApplicant, user.IsRegularManager, IsPrivileged(user)))
        {
            throw new ForbiddenException(
                "You cannot upload files for this admission, or the admission is closed.");
        }

        await EnsurePdfAsync(content, cancellationToken);

        var key = $"{kind.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}.pdf";
        var file = StoredFile.Create(
            applicantId,
            user.Id,
            user.Role,
            kind,
            fileName,
            key,
            content.Length);

        content.Position = 0;
        await _storage.UploadAsync(key, content, PdfContentType, cancellationToken);

        _files.Add(file);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(file);
    }

    public async Task<IReadOnlyList<StoredFileResponse>> GetForApplicantAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default)
    {
        var user = _currentUser.Get();
        var access = await GetAccessAsync(applicantId, cancellationToken);

        if (!access.AllowsRead(user.Id, user.IsApplicant, user.IsRegularManager, IsPrivileged(user)))
        {
            throw new ForbiddenException("You cannot view files for this admission.");
        }

        var files = await _files.GetByApplicantAsync(applicantId, cancellationToken);
        return files.Select(Map).ToList();
    }

    public async Task<FileDownload> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await GetAuthorizedFileAsync(fileId, requireOpenAdmission: false, cancellationToken);
        var stream = await _storage.DownloadAsync(file.StorageKey, cancellationToken);

        return new FileDownload(stream, file.OriginalFileName, PdfContentType);
    }

    public async Task ReplaceAsync(
        Guid fileId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var file = await GetAuthorizedFileAsync(fileId, requireOpenAdmission: true, cancellationToken);

        await EnsurePdfAsync(content, cancellationToken);

        content.Position = 0;
        await _storage.UploadAsync(file.StorageKey, content, PdfContentType, cancellationToken);

        file.Replace(fileName, content.Length);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await GetAuthorizedFileAsync(fileId, requireOpenAdmission: true, cancellationToken);

        await _storage.DeleteAsync(file.StorageKey, cancellationToken);

        _files.Remove(file);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<StoredFile> GetAuthorizedFileAsync(
        Guid fileId,
        bool requireOpenAdmission,
        CancellationToken cancellationToken)
    {
        var user = _currentUser.Get();

        var file = await _files.GetByIdAsync(fileId, cancellationToken)
            ?? throw NotFoundException.For<StoredFile>(fileId);

        var access = await GetAccessAsync(file.ApplicantId, cancellationToken);

        var allowed = requireOpenAdmission
            ? access.AllowsUpload(user.Id, user.IsApplicant, user.IsRegularManager, IsPrivileged(user))
            : access.AllowsRead(user.Id, user.IsApplicant, user.IsRegularManager, IsPrivileged(user));

        if (!allowed)
        {
            throw new ForbiddenException("You cannot access this file.");
        }

        return file;
    }

    private async Task<AdmissionAccess> GetAccessAsync(Guid applicantId, CancellationToken cancellationToken) =>
        await _access.GetAsync(applicantId, cancellationToken)
        ?? throw new NotFoundException(
            "No admission is known for this applicant yet. Please retry shortly.");

    private static bool IsPrivileged(CurrentUser user) => user.IsHeadManager || user.IsAdmin;

    private static async Task EnsurePdfAsync(Stream content, CancellationToken cancellationToken)
    {
        if (!content.CanSeek)
        {
            throw new ValidationException(nameof(content), "The uploaded stream must be seekable.");
        }

        if (content.Length == 0)
        {
            throw new ValidationException(nameof(content), "The uploaded file is empty.");
        }

        if (content.Length > StoredFile.MaxSizeBytes)
        {
            throw new ValidationException(
                nameof(content),
                $"The file exceeds the maximum size of {StoredFile.MaxSizeBytes / (1024 * 1024)} MB.");
        }

        content.Position = 0;
        var header = new byte[PdfMagicNumber.Length];
        var read = await content.ReadAsync(header, cancellationToken);
        content.Position = 0;

        if (read < PdfMagicNumber.Length || !header.SequenceEqual(PdfMagicNumber))
        {
            throw new ValidationException(nameof(content), "Only PDF files are accepted.");
        }
    }

    private static StoredFileResponse Map(StoredFile file) =>
        new(
            file.Id,
            file.ApplicantId,
            file.Kind,
            file.OriginalFileName,
            file.SizeBytes,
            file.CreatedAtUtc,
            file.ModifiedAtUtc);
}
