using FluentAssertions;

using MediatR;

using Moq;

using NUnit.Framework;

using OneTimePassGen.Application.Common.Behaviors;
using OneTimePassGen.Application.Common.Exceptions;
using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.Common.Security;

namespace OneTimePassGen.Application.UnitTests.Common.Behaviors;

public sealed class AuthorizationBehaviorTests
{
    private Mock<ICurrentUserService> _currentUserService = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _currentUserService = new Mock<ICurrentUserService>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_NextHandler_When_UnauthorizedQuery_AnonymousUser()
    {
        // Arrange
        AuthorizationBehavior<UnauthorizedQuery, Unit> authBehavior = new(currentUserService: null!, identityService: null!);

        bool nextHandlerWasTriggered = false;

        Task<Unit> Next()
        {
            nextHandlerWasTriggered = true;
            return Task.FromResult(Unit.Value);
        }

        // Act
        await authBehavior.Handle(
            request: new UnauthorizedQuery(),
            cancellationToken: new CancellationToken(),
            next: Next);

        // Assert
        nextHandlerWasTriggered
            .Should()
            .BeTrue();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldThrow_UnauthorizedAccessException_When_AuthorizedOnlyQuery_AnonymousUser()
    {
        // Arrange
        _currentUserService
            .Setup(x => x.UserId)
            .Returns((string)null!);

        AuthorizationBehavior<AuthorizedOnlyQuery, Unit> authBehavior
            = new(currentUserService: _currentUserService.Object, identityService: null!);

        // Act
        Task<Unit> request = authBehavior.Handle(
            request: new AuthorizedOnlyQuery(),
            cancellationToken: new CancellationToken(),
            next: null!);

        // Assert
        await FluentActions
            .Invoking(() => request)
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_NextHandler_When_AuthorizedOnlyQuery_AuthenticatedUser()
    {
        // Arrange
        _currentUserService
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        AuthorizationBehavior<AuthorizedOnlyQuery, Unit> authBehavior
            = new(currentUserService: _currentUserService.Object, identityService: null!);

        bool nextHandlerWasTriggered = false;

        Task<Unit> Next()
        {
            nextHandlerWasTriggered = true;
            return Task.FromResult(Unit.Value);
        }

        // Act
        Unit request = await authBehavior.Handle(
            request: new AuthorizedOnlyQuery(),
            cancellationToken: new CancellationToken(),
            next: Next);

        // Assert
        nextHandlerWasTriggered
            .Should()
            .BeTrue();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_IsInRoleAsync_When_AdminRoleQuery_UserWithAdminRole()
    {
        // Arrange
        const string roleName = "Admin";
        string userId = Guid.NewGuid().ToString();

        _currentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        _identityService
            .Setup(_ => _.IsInRoleAsync(userId, roleName))
            .Returns(Task.FromResult(true));

        AuthorizationBehavior<AdminRoleQuery, Unit> authBehavior
            = new(currentUserService: _currentUserService.Object, identityService: _identityService.Object);

        // Act
        await authBehavior.Handle(
            request: new AdminRoleQuery(),
            cancellationToken: new CancellationToken(),
            next: () => Task.FromResult(Unit.Value));

        // Assert
        _identityService.Verify(i => i.IsInRoleAsync(userId, roleName), Times.Once);
    }

    public async Task AuthorizationBehavior_ShouldThrow_ForbiddenAccessException_When_AdminRoleQuery_UserWithoutRoles()
    {
        // Arrange
        const string roleName = "Admin";
        string userId = Guid.NewGuid().ToString();

        _currentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        _identityService
            .Setup(_ => _.IsInRoleAsync(userId, roleName))
            .Returns(Task.FromResult(false));

        AuthorizationBehavior<AdminRoleQuery, Unit> authBehavior
            = new(currentUserService: _currentUserService.Object, identityService: _identityService.Object);

        // Act
        Task<Unit> request = authBehavior.Handle(
            request: new AdminRoleQuery(),
            cancellationToken: new CancellationToken(),
            next: () => Task.FromResult(Unit.Value));

        // Assert
        await FluentActions
            .Invoking(() => request)
            .Should()
            .ThrowAsync<ForbiddenAccessException>();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_AuthorizeAsync_When_AdminPolicyQuery_UserWithAdminPolicy()
    {
        // Arrange
        const string policyName = "AdminPolicy";
        string userId = Guid.NewGuid().ToString();

        _currentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        _identityService
            .Setup(_ => _.AuthorizeAsync(userId, policyName))
            .Returns(Task.FromResult(true));

        AuthorizationBehavior<AdminPolicyQuery, Unit> authBehavior
            = new(currentUserService: _currentUserService.Object, identityService: _identityService.Object);

        // Act
        await authBehavior.Handle(
            request: new AdminPolicyQuery(),
            cancellationToken: new CancellationToken(),
            next: () => Task.FromResult(Unit.Value));

        // Assert
        _identityService.Verify(i => i.AuthorizeAsync(userId, policyName), Times.Once);
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldThrow_ForbiddenAccessException_When_AdminPolicyQuery_UserWithoutAdminPolicy()
    {
        // Arrange
        const string policyName = "AdminPolicy";
        string userId = Guid.NewGuid().ToString();

        _currentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        _identityService
            .Setup(_ => _.AuthorizeAsync(userId, policyName))
            .Returns(Task.FromResult(false));

        AuthorizationBehavior<AdminPolicyQuery, Unit> authBehavior
            = new(currentUserService: _currentUserService.Object, identityService: _identityService.Object);

        // Act
        Task<Unit> request = authBehavior.Handle(
            request: new AdminPolicyQuery(),
            cancellationToken: new CancellationToken(),
            next: null!);

        // Assert
        await FluentActions
            .Invoking(() => request)
            .Should()
            .ThrowAsync<ForbiddenAccessException>();
    }

    sealed class UnauthorizedQuery : IRequest<Unit>
    {
    }

    [Authorize]
    sealed class AuthorizedOnlyQuery : IRequest<Unit>
    {
    }

    [Authorize(Roles = "Admin")]
    sealed class AdminRoleQuery : IRequest<Unit>
    {
    }

    [Authorize(Policy = "AdminPolicy")]
    sealed class AdminPolicyQuery : IRequest<Unit>
    {
    }
}