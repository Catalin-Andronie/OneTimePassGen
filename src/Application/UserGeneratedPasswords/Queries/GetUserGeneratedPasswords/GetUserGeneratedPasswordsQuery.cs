using OneTimePassGen.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneTimePassGen.Application.Common.Security;
using OneTimePassGen.Application.UserGeneratedPasswords.Models;

namespace OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPasswords;
#pragma warning disable MA0048 // File name must match type name

[Authorize]
public sealed class GetUserGeneratedPasswordsQuery : IRequest<IList<UserGeneratedPasswordItem>>
{
    public GetUserGeneratedPasswordsQuery(bool? includeExpiredPasswords)
    {
        IncludeExpiredPasswords = includeExpiredPasswords ?? false;
    }

    public readonly bool IncludeExpiredPasswords;
}

internal sealed class GetUserGeneratedPasswordsQueryHandler : IRequestHandler<GetUserGeneratedPasswordsQuery, IList<UserGeneratedPasswordItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public GetUserGeneratedPasswordsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _dbContext = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<IList<UserGeneratedPasswordItem>> Handle(GetUserGeneratedPasswordsQuery request, CancellationToken cancellationToken)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        var query = _dbContext.UserGeneratedPasswords.Where(p => p.UserId == currentUserId);

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
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
    }
}