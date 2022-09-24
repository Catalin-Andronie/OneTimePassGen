using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.UserGeneratedPasswords.Models;
using OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPasswords;
using OneTimePassGen.Domain.Entities;
using OneTimePassGen.Infrastructure.Persistance;

namespace OneTimePassGen.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/user-generated-passwords")]
[Produces("application/json")]
public sealed class UserGeneratedPasswordController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UserGeneratedPasswordController(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

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
    public async Task<ActionResult<UserGeneratedPasswordItem>> GetUserPasswordAsync(Guid userPasswordId, CancellationToken cancellationToken = default)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        var generatedPassword = await _dbContext.UserGeneratedPasswords
                                            .Where(p => p.Id == userPasswordId && p.UserId == currentUserId)
                                            .Select(p => new UserGeneratedPasswordItem
                                            {
                                                Id = p.Id,
                                                Password = p.Password,
                                                ExpiresAt = p.ExpiersAt,
                                                CreatedAt = p.CreatedAt
                                            })
                                            .SingleOrDefaultAsync(cancellationToken)
                                            .ConfigureAwait(false);

        return generatedPassword ?? (ActionResult<UserGeneratedPasswordItem>)NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserGeneratedPasswordItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGeneratedPasswordItem>> CreateUserPasswordAsync(CancellationToken cancellationToken = default)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        string passwordValue = Guid.NewGuid().ToString();
        var now = DateTimeOffset.Now;
        const double generatedPasswordExpirationSeconds = 30;
        var expiresAt = now.AddSeconds(generatedPasswordExpirationSeconds);

        var generatedPassword = new UserGeneratedPassword
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Password = passwordValue,
            CreatedAt = now,
            ExpiersAt = expiresAt
        };

        await _dbContext.UserGeneratedPasswords.AddAsync(generatedPassword, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var generatedPasswordDto = new UserGeneratedPasswordItem
        {
            Id = generatedPassword.Id,
            Password = generatedPassword.Password,
            ExpiresAt = generatedPassword.ExpiersAt,
            CreatedAt = generatedPassword.CreatedAt
        };
        return CreatedAtAction(nameof(GetUserPasswordAsync), new { generatedPasswordDto.Id }, generatedPasswordDto);
    }
}