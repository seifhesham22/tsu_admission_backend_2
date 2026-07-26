using Files.Application.Files.Contracts;
using Files.Application.Files.Dtos;
using Files.Application.Files;
using Files.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace Files.Api.Controllers;

[ApiController]
[Route("api/v1/files")]
[Authorize(Roles = Roles.AnyAuthenticated)]
[Produces("application/json")]
public sealed class FilesController : ControllerBase
{
    private const long MaxUploadBytes = StoredFile.MaxSizeBytes;

    private readonly IFileService _files;

    public FilesController(IFileService files)
    {
        _files = files;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StoredFileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StoredFileResponse>>> GetForApplicant(
        [FromQuery] Guid applicantId,
        CancellationToken cancellationToken) =>
        Ok(await _files.GetForApplicantAsync(applicantId, cancellationToken));

    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(StoredFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StoredFileResponse>> Upload(
        [FromQuery] Guid applicantId,
        [FromQuery] FileKind kind,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "A file is required."
            });
        }

        await using var buffer = new MemoryStream();
        await using (var source = file.OpenReadStream())
        {
            await source.CopyToAsync(buffer, cancellationToken);
        }

        buffer.Position = 0;

        var stored = await _files.UploadAsync(
            applicantId,
            kind,
            file.FileName,
            buffer,
            cancellationToken);

        return CreatedAtAction(nameof(Download), new { fileId = stored.Id }, stored);
    }

    [HttpGet("{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
    {
        var download = await _files.DownloadAsync(fileId, cancellationToken);
        return File(download.Content, download.ContentType, download.FileName);
    }

    [HttpPut("{fileId:guid}")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Replace(
        Guid fileId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "A file is required."
            });
        }

        await using var buffer = new MemoryStream();
        await using (var source = file.OpenReadStream())
        {
            await source.CopyToAsync(buffer, cancellationToken);
        }

        buffer.Position = 0;

        await _files.ReplaceAsync(fileId, file.FileName, buffer, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid fileId, CancellationToken cancellationToken)
    {
        await _files.DeleteAsync(fileId, cancellationToken);
        return NoContent();
    }
}
