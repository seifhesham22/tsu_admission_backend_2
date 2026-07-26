using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace Admission.Api.Controllers;

[ApiController]
[Route("api/v1/applicants/me/admission")]
[Authorize(Roles = Roles.Applicant)]
[Produces("application/json")]
public sealed class MyAdmissionController : ControllerBase
{
    private readonly IAdmissionService _admissions;

    public MyAdmissionController(IAdmissionService admissions)
    {
        _admissions = admissions;
    }

    [HttpGet("programs")]
    [ProducesResponseType(typeof(IReadOnlyList<SelectedProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SelectedProgramResponse>>> GetPrograms(
        CancellationToken cancellationToken) =>
        Ok(await _admissions.GetMyProgramsAsync(cancellationToken));

    [HttpPost("programs")]
    [ProducesResponseType(typeof(IReadOnlyList<SelectedProgramResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SelectProgram(
        [FromBody] SelectProgramRequest request,
        CancellationToken cancellationToken)
    {
        var admissionProgramId = await _admissions.SelectProgramAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPrograms), new { admissionProgramId }, null);
    }

    [HttpDelete("programs/{admissionProgramId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProgram(
        Guid admissionProgramId,
        CancellationToken cancellationToken)
    {
        await _admissions.RemoveProgramAsync(admissionProgramId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("programs/{admissionProgramId:guid}/priority")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePriority(
        Guid admissionProgramId,
        [FromBody] ChangeProgramPriorityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _admissions.ChangePriorityAsync(admissionProgramId, request.Priority, cancellationToken);
        return NoContent();
    }
}
