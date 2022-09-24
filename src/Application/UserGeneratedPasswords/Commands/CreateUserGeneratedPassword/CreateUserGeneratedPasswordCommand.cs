using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Domain.Entities;

using MediatR;
using OneTimePassGen.Application.Common.Security;

namespace OneTimePassGen.Application.UserGeneratedPasswords.Commands.CreateUserGeneratedPassword;
#pragma warning disable MA0048 // File name must match type name

[Authorize]
public sealed class CreateUserGeneratedPasswordCommand : IRequest<Guid>
{
}

internal sealed class CreateUserGeneratedPasswordCommandHandler : IRequestHandler<CreateUserGeneratedPasswordCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateUserGeneratedPasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateUserGeneratedPasswordCommand request, CancellationToken cancellationToken = default)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        string passwordValue = Guid.NewGuid().ToString();
        var now = DateTimeOffset.Now;

        // TODO: Move `GeneratedPasswordExpirationSeconds` value to config file.
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
        return generatedPassword.Id;
    }
}