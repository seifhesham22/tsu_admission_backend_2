using Admission.Application.Managers.Dtos;
using Admission.Application.Managers.Contracts;
using Admission.Application.Managers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Kernel.Pagination;

namespace Admission.Api.Controllers;

[ApiController]
[Route("api/v1/managers")]
[Authorize(Roles = Roles.HeadManager)]
[Produces("application/json")]
public sealed class ManagersController : ControllerBase
{
    private readonly IHeadManagerService _headManager;

    public ManagersController(IHeadManagerService headManager)
    {
        _headManager = headManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ManagerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ManagerResponse>>> GetManagers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var page = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
        return Ok(await _headManager.GetManagersAsync(page, cancellationToken));
    }
}
