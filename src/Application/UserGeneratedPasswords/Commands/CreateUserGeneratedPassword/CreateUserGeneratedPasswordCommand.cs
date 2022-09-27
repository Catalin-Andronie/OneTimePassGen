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
    private readonly IDateTime _dateTime;
    private readonly IApplicationConfiguration _configuration;

    public CreateUserGeneratedPasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        IApplicationConfiguration configuration)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _configuration = configuration;
    }

    public async Task<Guid> Handle(CreateUserGeneratedPasswordCommand request, CancellationToken cancellationToken = default)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        // TODO: Decouple away the password generation so that we can provide any implementations.
        string passwordValue = Guid.NewGuid().ToString();
        var now = _dateTime.Now;

        var generatedPasswordExpirationSeconds = _configuration.GetGeneratedPasswordExpirationSeconds();
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