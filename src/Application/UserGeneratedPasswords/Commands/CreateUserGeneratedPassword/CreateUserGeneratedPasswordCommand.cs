using MediatR;

using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.Common.Security;
using OneTimePassGen.Domain.Entities;

namespace OneTimePassGen.Application.UserGeneratedPasswords.Commands.CreateUserGeneratedPassword;
#pragma warning disable MA0048 // File name must match type name

[Authorize]
public sealed class CreateUserGeneratedPasswordCommand
    : IRequest<Guid>
{
}

internal sealed class CreateUserGeneratedPasswordCommandHandler
    : IRequestHandler<CreateUserGeneratedPasswordCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IApplicationConfiguration _configuration;
    private readonly IPasswordGenerator _passwordGenerator;

    public CreateUserGeneratedPasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        IPasswordGenerator passwordGenerator,
        IApplicationConfiguration configuration)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _configuration = configuration;
        _passwordGenerator = passwordGenerator;
    }

    public async Task<Guid> Handle(
        CreateUserGeneratedPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        string currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");

        // TODO: Decouple away the password generation so that we can provide any implementations.
        string passwordValue = _passwordGenerator.GeneratePassword();
        DateTimeOffset now = _dateTime.Now;

        int generatedPasswordExpirationSeconds = _configuration.GetGeneratedPasswordExpirationSeconds();
        DateTimeOffset expiresAt = now.AddSeconds(generatedPasswordExpirationSeconds);

        UserGeneratedPassword generatedPassword = new()
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Password = passwordValue,
            CreatedAt = now,
            ExpiersAt = expiresAt
        };

        await _dbContext
            .UserGeneratedPasswords
            .AddAsync(generatedPassword, cancellationToken)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return generatedPassword.Id;
    }
}