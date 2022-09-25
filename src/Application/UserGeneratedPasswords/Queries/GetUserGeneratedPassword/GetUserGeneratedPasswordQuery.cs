using OneTimePassGen.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneTimePassGen.Application.Common.Security;
using OneTimePassGen.Application.UserGeneratedPasswords.Models;

namespace OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPassword;
#pragma warning disable MA0048 // File name must match type name

[Authorize]
public sealed class GetUserGeneratedPasswordQuery : IRequest<UserGeneratedPasswordItem?>
{
    public GetUserGeneratedPasswordQuery(Guid generatedPasswordId,
                                         bool? includeExpiredPasswords)
    {
        GeneratedPasswordId = generatedPasswordId;
        IncludeExpiredPasswords = includeExpiredPasswords ?? false;
    }

    public readonly Guid GeneratedPasswordId;
    public readonly bool IncludeExpiredPasswords;
}

internal sealed class GetUserGeneratedPasswordQueryHandler : IRequestHandler<GetUserGeneratedPasswordQuery, UserGeneratedPasswordItem?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public GetUserGeneratedPasswordQueryHandler(IApplicationDbContext context,
                                                ICurrentUserService currentUserService,
                                                IDateTime dateTime)
    {
        _dbContext = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<UserGeneratedPasswordItem?> Handle(GetUserGeneratedPasswordQuery request, CancellationToken cancellationToken)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        var query = _dbContext.UserGeneratedPasswords.Where(p => p.Id == request.GeneratedPasswordId &&
                                                                 p.UserId == currentUserId);

        if (!request.IncludeExpiredPasswords)
        {
            var now = _dateTime.Now;
            query = query.Where(p => p.ExpiersAt > now);
        }

        return await query
                    .Select(p => new UserGeneratedPasswordItem
                    {
                        Id = p.Id,
                        Password = p.Password,
                        ExpiresAt = p.ExpiersAt,
                        CreatedAt = p.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
    }
}