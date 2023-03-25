using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OneTimePassGen.Application.UserGeneratedPasswords.Commands.CreateUserGeneratedPassword;
using OneTimePassGen.Application.UserGeneratedPasswords.Models;
using OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPassword;
using OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPasswords;

namespace OneTimePassGen.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/user-generated-passwords")]
[Produces("application/json")]
public sealed class UserGeneratedPasswordController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IList<UserGeneratedPasswordItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IList<UserGeneratedPasswordItem>> GetUserPasswordsAsync(
        [FromQuery] bool? includeExpiredPasswords,
        CancellationToken cancellationToken = default)
    {
        GetUserGeneratedPasswordsQuery getAllRequest = new(includeExpiredPasswords);
        return await Mediator.Send(getAllRequest, cancellationToken);
    }

    [HttpGet("{generatedPasswordId}")]
    [ProducesResponseType(typeof(IList<UserGeneratedPasswordItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGeneratedPasswordItem>> GetUserPasswordAsync(
        Guid generatedPasswordId,
        [FromQuery] bool? includeExpiredPasswords,
        CancellationToken cancellationToken = default)
    {
        GetUserGeneratedPasswordQuery getRequest = new(generatedPasswordId, includeExpiredPasswords);
        UserGeneratedPasswordItem? userGeneratedPasswordItem = await Mediator.Send(getRequest, cancellationToken);

        return userGeneratedPasswordItem ?? (ActionResult<UserGeneratedPasswordItem>)NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserGeneratedPasswordItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGeneratedPasswordItem>> CreateUserPasswordAsync(
        CreateUserGeneratedPasswordCommand createCommand,
        CancellationToken cancellationToken = default)
    {
        Guid generatedPasswordId = await Mediator.Send(createCommand, cancellationToken);
        if (generatedPasswordId == Guid.Empty)
            return NoContent();

        GetUserGeneratedPasswordQuery getRequest = new(generatedPasswordId, includeExpiredPasswords: false);
        UserGeneratedPasswordItem? userGeneratedPasswordItem = await Mediator.Send(getRequest, cancellationToken);

        return CreatedAtAction(nameof(GetUserPasswordAsync), new { userGeneratedPasswordItem?.Id }, userGeneratedPasswordItem);
    }
}