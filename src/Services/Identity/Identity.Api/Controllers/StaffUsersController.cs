using Identity.Api.Admin;
using Identity.Api.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Kernel.Pagination;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/v1/staff-users")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public sealed class StaffUsersController : ControllerBase
{
    private readonly IAdminUserService _admin;

    public StaffUsersController(IAdminUserService admin)
    {
        _admin = admin;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StaffUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<StaffUserResponse>>> GetStaff(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var page = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
        return Ok(await _admin.GetStaffAsync(page, cancellationToken));
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<string>> GetAssignableRoles() =>
        Ok(_admin.GetAssignableRoles());

    [HttpPost("managers")]
    [ProducesResponseType(typeof(StaffUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffUserResponse>> CreateManager(
        [FromBody] CreateStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _admin.CreateAsync(request, Roles.RegularManager, cancellationToken);
        return CreatedAtAction(nameof(GetStaff), new { pageNumber = 1 }, user);
    }

    [HttpPost("head-managers")]
    [ProducesResponseType(typeof(StaffUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffUserResponse>> CreateHeadManager(
        [FromBody] CreateStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _admin.CreateAsync(request, Roles.HeadManager, cancellationToken);
        return CreatedAtAction(nameof(GetStaff), new { pageNumber = 1 }, user);
    }

    [HttpPatch("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStaffUser(
        Guid userId,
        [FromBody] UpdateStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        await _admin.UpdateAsync(userId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStaffUser(Guid userId, CancellationToken cancellationToken)
    {
        await _admin.DeleteAsync(userId, cancellationToken);
        return NoContent();
    }
}
