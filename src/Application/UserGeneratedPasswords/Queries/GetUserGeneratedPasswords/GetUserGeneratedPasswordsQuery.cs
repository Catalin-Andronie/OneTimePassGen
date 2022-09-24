using OneTimePassGen.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPasswords;
#pragma warning disable MA0048 // File name must match type name

public sealed class GetUserGeneratedPasswordsQuery : IRequest<IList<UserGeneratedPasswordDto>>
{
    public bool IncludeExpiredPasswords { get; set; }
}

internal sealed class GetUserGeneratedPasswordsQueryHandler : IRequestHandler<GetUserGeneratedPasswordsQuery, IList<UserGeneratedPasswordDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUserGeneratedPasswordsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _dbContext = context;
        _currentUserService = currentUserService;
    }

    public async Task<IList<UserGeneratedPasswordDto>> Handle(GetUserGeneratedPasswordsQuery request, CancellationToken cancellationToken)
    {
        string currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User unauthorized to execute this action.");
        var query = _dbContext.UserGeneratedPasswords.Where(p => p.UserId == currentUserId);

        if (!request.IncludeExpiredPasswords)
        {
            var now = DateTimeOffset.Now;
            query = query.Where(p => p.ExpiersAt > now);
        }

        return await query
                    .Select(p => new UserGeneratedPasswordDto
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