using System.Reflection;

using MediatR;

using OneTimePassGen.Application.Common.Exceptions;
using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.Common.Security;

namespace OneTimePassGen.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        IEnumerable<AuthorizeAttribute> authorizeAttributes = request
            .GetType()
            .GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_currentUserService.UserId == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            IEnumerable<AuthorizeAttribute> authorizeAttributesWithRoles = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                bool authorized = false;

                foreach (string[]? roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (string role in roles)
                    {
                        bool isInRole = await _identityService
                            .IsInRoleAsync(_currentUserService.UserId, role.Trim())
                            .ConfigureAwait(false);

                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                // Must be a member of at least one role in roles
                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }

            // Policy-based authorization
            IEnumerable<AuthorizeAttribute> authorizeAttributesWithPolicies = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.Policy));

            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (string? policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    bool authorized = await _identityService
                        .AuthorizeAsync(_currentUserService.UserId, policy)
                        .ConfigureAwait(false);

                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next().ConfigureAwait(false);
    }
}