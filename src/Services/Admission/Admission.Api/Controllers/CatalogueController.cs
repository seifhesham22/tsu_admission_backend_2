using Admission.Application.Catalogue.Dtos;
using Admission.Application.Catalogue.Contracts;
using Admission.Application.Catalogue.Services;
using Admission.Application.Sync.Dtos;
using Admission.Application.Sync.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Kernel.Pagination;

namespace Admission.Api.Controllers;

[ApiController]
[Route("api/v1/catalogue")]
[AllowAnonymous]
[Produces("application/json")]
public sealed class CatalogueController : ControllerBase
{
    private readonly ICatalogueService _catalogue;
    private readonly ICatalogueSyncService _sync;

    public CatalogueController(ICatalogueService catalogue, ICatalogueSyncService sync)
    {
        _catalogue = catalogue;
        _sync = sync;
    }

    [HttpGet("faculties")]
    [ProducesResponseType(typeof(IReadOnlyList<FacultyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FacultyResponse>>> GetFaculties(
        CancellationToken cancellationToken) =>
        Ok(await _catalogue.GetFacultiesAsync(cancellationToken));

    [HttpGet("education-levels")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationLevelResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EducationLevelResponse>>> GetEducationLevels(
        CancellationToken cancellationToken) =>
        Ok(await _catalogue.GetEducationLevelsAsync(cancellationToken));

    [HttpGet("education-forms")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetEducationForms(
        CancellationToken cancellationToken) =>
        Ok(await _catalogue.GetEducationFormsAsync(cancellationToken));

    [HttpGet("languages")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetLanguages(
        CancellationToken cancellationToken) =>
        Ok(await _catalogue.GetLanguagesAsync(cancellationToken));

    [HttpGet("document-types")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationDocumentTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EducationDocumentTypeResponse>>> GetDocumentTypes(
        CancellationToken cancellationToken) =>
        Ok(await _catalogue.GetDocumentTypesAsync(cancellationToken));

    [HttpGet("priorities")]
    [ProducesResponseType(typeof(IReadOnlyList<PriorityOption>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<PriorityOption>> GetPriorities() =>
        Ok(_catalogue.GetPriorities());

    [HttpGet("admission-statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<AdmissionStatusOption>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdmissionStatusOption>> GetAdmissionStatuses() =>
        Ok(_catalogue.GetAdmissionStatuses());

    [HttpGet("programs")]
    [ProducesResponseType(typeof(PagedResult<EducationProgramResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EducationProgramResponse>>> GetPrograms(
        [FromQuery] Guid[]? facultyIds,
        [FromQuery] Guid[]? educationLevelIds,
        [FromQuery] string[]? languages,
        [FromQuery] string[]? educationForms,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = new EducationProgramFilter
        {
            FacultyIds = facultyIds,
            EducationLevelIds = educationLevelIds,
            Languages = languages,
            EducationForms = educationForms,
            Search = search,
            Page = new PageRequest { PageNumber = pageNumber, PageSize = pageSize }
        };

        return Ok(await _catalogue.SearchProgramsAsync(filter, cancellationToken));
    }

    [HttpPost("sync")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CatalogueSyncResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CatalogueSyncResult>> Sync(CancellationToken cancellationToken) =>
        Ok(await _sync.SyncAllAsync(cancellationToken));
}
