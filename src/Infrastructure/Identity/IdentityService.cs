using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.Common.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace OneTimePassGen.Infrastructure.Identity;

internal sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        ApplicationUser user = await _userManager
            .Users
            .FirstAsync(u => u.Id == userId)
            .ConfigureAwait(false);

        return user.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(
        string userName,
        string password)
    {
        ApplicationUser user = new()
        {
            UserName = userName,
            Email = userName,
        };

        IdentityResult result = await _userManager
            .CreateAsync(user, password)
            .ConfigureAwait(false);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        ApplicationUser? user = _userManager
            .Users
            .SingleOrDefault(u => u.Id == userId);

        bool isInRole = false;

        if (user != null)
        {
            isInRole = await _userManager
                .IsInRoleAsync(user, role)
                .ConfigureAwait(false);
        }

        return isInRole;
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        ApplicationUser? user = await _userManager
            .Users
            .SingleOrDefaultAsync(u => u.Id == userId)
            .ConfigureAwait(false);

        if (user == null)
        {
            return false;
        }

        ClaimsPrincipal principal = await _userClaimsPrincipalFactory
            .CreateAsync(user)
            .ConfigureAwait(false);

        AuthorizationResult result = await _authorizationService
            .AuthorizeAsync(principal, policyName)
            .ConfigureAwait(false);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        ApplicationUser? user = await _userManager
            .Users
            .SingleOrDefaultAsync(u => u.Id == userId)
            .ConfigureAwait(false);

        return user != null
            ? await DeleteUserAsync(user).ConfigureAwait(false)
            : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        IdentityResult result = await _userManager
            .DeleteAsync(user)
            .ConfigureAwait(false);

        return result.ToApplicationResult();
    }
}