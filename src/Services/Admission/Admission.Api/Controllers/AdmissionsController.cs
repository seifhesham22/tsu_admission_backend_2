using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Services;
using Admission.Application.Managers.Dtos;
using Admission.Application.Managers.Contracts;
using Admission.Application.Managers.Services;
using Admission.Domain.Admissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Kernel.Pagination;

namespace Admission.Api.Controllers;

[ApiController]
[Route("api/v1/admissions")]
[Authorize(Roles = Roles.AnyManagerOrAdmin)]
[Produces("application/json")]
public sealed class AdmissionsController : ControllerBase
{
    private readonly IManagerWorkloadService _workload;
    private readonly IAdmissionService _admissions;
    private readonly IHeadManagerService _headManager;

    public AdmissionsController(
        IManagerWorkloadService workload,
        IAdmissionService admissions,
        IHeadManagerService headManager)
    {
        _workload = workload;
        _admissions = admissions;
        _headManager = headManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdmissionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdmissionSummaryResponse>>> Search(
        [FromQuery] Guid? educationProgramId,
        [FromQuery] Guid[]? facultyIds,
        [FromQuery] AdmissionStatus? status,
        [FromQuery] bool onlyUnassigned,
        [FromQuery] bool onlyMine,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = new AdmissionFilter
        {
            EducationProgramId = educationProgramId,
            FacultyIds = facultyIds,
            Status = status,
            OnlyUnassigned = onlyUnassigned,
            OnlyMine = onlyMine,
            Search = search,
            Page = new PageRequest { PageNumber = pageNumber, PageSize = pageSize }
        };

        return Ok(await _workload.SearchAdmissionsAsync(filter, cancellationToken));
    }

    [HttpPost("{admissionId:guid}/manager")]
    [Authorize(Roles = Roles.AnyManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TakeOwnership(Guid admissionId, CancellationToken cancellationToken)
    {
        await _workload.TakeOwnershipAsync(admissionId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{admissionId:guid}/manager")]
    [Authorize(Roles = Roles.AnyManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseOwnership(Guid admissionId, CancellationToken cancellationToken)
    {
        await _workload.ReleaseOwnershipAsync(admissionId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{admissionId:guid}/manager")]
    [Authorize(Roles = Roles.HeadManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignManager(
        Guid admissionId,
        [FromBody] AssignManagerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _headManager.AssignManagerAsync(admissionId, request.ManagerId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{admissionId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid admissionId,
        [FromBody] ChangeAdmissionStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _workload.ChangeStatusAsync(admissionId, request.Status, cancellationToken);
        return NoContent();
    }

    [HttpDelete("applicants/{applicantId:guid}/programs/{admissionProgramId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveApplicantProgram(
        Guid applicantId,
        Guid admissionProgramId,
        CancellationToken cancellationToken)
    {
        await _admissions.RemoveProgramForApplicantAsync(applicantId, admissionProgramId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("applicants/{applicantId:guid}/programs/{admissionProgramId:guid}/priority")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeApplicantProgramPriority(
        Guid applicantId,
        Guid admissionProgramId,
        [FromBody] ChangeProgramPriorityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _admissions.ChangePriorityForApplicantAsync(
            applicantId,
            admissionProgramId,
            request.Priority,
            cancellationToken);

        return NoContent();
    }
}
