using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
[Route("api/v1/users")]
public sealed class UsersController(IListUsers list, IUpdateUser update) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedUsers>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedUsers>> List(CancellationToken cancellationToken,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await list.ExecuteAsync(page, pageSize, cancellationToken));

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.UsersUpdate)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        Ok(await update.ExecuteAsync(new UpdateUserCommand(id, request.Name, request.Email, request.IsActive!.Value), cancellationToken));
}
