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
        [FromQuery] GetUserGeneratedPasswordsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await Mediator.Send(query, cancellationToken);
    }

    [HttpGet("{userPasswordId}")]
    [ProducesResponseType(typeof(IList<UserGeneratedPasswordItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGeneratedPasswordItem>> GetUserPasswordAsync(Guid generatedPasswordId, CancellationToken cancellationToken = default)
    {
        var getRequest = new GetUserGeneratedPasswordQuery { Id = generatedPasswordId };
        var userGeneratedPasswordItem = await Mediator.Send(getRequest, cancellationToken);

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
        var generatedPasswordId = await Mediator.Send(createCommand, cancellationToken);
        if (generatedPasswordId == Guid.Empty)
            return NoContent();

        var getRequest = new GetUserGeneratedPasswordQuery { Id = generatedPasswordId };
        var userGeneratedPasswordItem = await Mediator.Send(getRequest, cancellationToken);

        return CreatedAtAction(nameof(GetUserPasswordAsync), new { userGeneratedPasswordItem?.Id }, userGeneratedPasswordItem);
    }
}