using OneTimePassGen.Application.Common.Behaviors;
using OneTimePassGen.Application.Common.Exceptions;
using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.Common.Security;

using FluentAssertions;

using MediatR;

using Moq;

using NUnit.Framework;

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
        var authBehavior = new AuthorizationBehavior<UnauthorizedQuery, Unit>(currentUserService: null!, identityService: null!);

        var nextHandlerWasTriggered = false;
        Task<Unit> Next()
        {
            nextHandlerWasTriggered = true;
            return Task.FromResult(Unit.Value);
        }

        // Act
        await authBehavior.Handle(new UnauthorizedQuery(), new CancellationToken(), Next);

        // Assert
        nextHandlerWasTriggered.Should().BeTrue();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldThrow_UnauthorizedAccessException_When_AuthorizedOnlyQuery_AnonymousUser()
    {
        // Arrange
        _currentUserService.Setup(x => x.UserId).Returns((string)null!);
        var authBehavior = new AuthorizationBehavior<AuthorizedOnlyQuery, Unit>(_currentUserService.Object, identityService: null!);

        // Act
        var request = authBehavior.Handle(new AuthorizedOnlyQuery(), new CancellationToken(), next: null!);

        // Assert
        await FluentActions.Invoking(() => request).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_NextHandler_When_AuthorizedOnlyQuery_AuthenticatedUser()
    {
        // Arrange
        _currentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        var authBehavior = new AuthorizationBehavior<AuthorizedOnlyQuery, Unit>(_currentUserService.Object, identityService: null!);

        var nextHandlerWasTriggered = false;

        Task<Unit> Next()
        {
            nextHandlerWasTriggered = true;
            return Task.FromResult(Unit.Value);
        }

        // Act
        var request = await authBehavior.Handle(new AuthorizedOnlyQuery(), new CancellationToken(), Next);

        // Assert
        nextHandlerWasTriggered.Should().BeTrue();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_IsInRoleAsync_When_AdminRoleQuery_UserWithAdminRole()
    {
        // Arrange
        const string roleName = "Admin";
        var userId = Guid.NewGuid().ToString();
        _currentUserService.Setup(x => x.UserId).Returns(userId);
        _identityService.Setup(_ => _.IsInRoleAsync(userId, roleName)).Returns(Task.FromResult(true));
        var authBehavior = new AuthorizationBehavior<AdminRoleQuery, Unit>(_currentUserService.Object, _identityService.Object);

        // Act
        await authBehavior.Handle(new AdminRoleQuery(), new CancellationToken(), () => Task.FromResult(Unit.Value));

        // Assert
        _identityService.Verify(i => i.IsInRoleAsync(userId, roleName), Times.Once);
    }

    public async Task AuthorizationBehavior_ShouldThrow_ForbiddenAccessException_When_AdminRoleQuery_UserWithoutRoles()
    {
        // Arrange
        const string roleName = "Admin";
        var userId = Guid.NewGuid().ToString();
        _currentUserService.Setup(x => x.UserId).Returns(userId);
        _identityService.Setup(_ => _.IsInRoleAsync(userId, roleName)).Returns(Task.FromResult(false));
        var authBehavior = new AuthorizationBehavior<AdminRoleQuery, Unit>(_currentUserService.Object, _identityService.Object);

        // Act
        var request = authBehavior.Handle(new AdminRoleQuery(), new CancellationToken(), () => Task.FromResult(Unit.Value));

        // Assert
        await FluentActions.Invoking(() => request).Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldCall_AuthorizeAsync_When_AdminPolicyQuery_UserWithAdminPolicy()
    {
        // Arrange
        const string policyName = "AdminPolicy";
        var userId = Guid.NewGuid().ToString();
        _currentUserService.Setup(x => x.UserId).Returns(userId);
        _identityService.Setup(_ => _.AuthorizeAsync(userId, policyName)).Returns(Task.FromResult(true));
        var authBehavior = new AuthorizationBehavior<AdminPolicyQuery, Unit>(_currentUserService.Object, _identityService.Object);

        // Act
        await authBehavior.Handle(new AdminPolicyQuery(), new CancellationToken(), () => Task.FromResult(Unit.Value));

        // Assert
        _identityService.Verify(i => i.AuthorizeAsync(userId, policyName), Times.Once);
    }

    [Test]
    public async Task AuthorizationBehavior_ShouldThrow_ForbiddenAccessException_When_AdminPolicyQuery_UserWithoutAdminPolicy()
    {
        // Arrange
        const string policyName = "AdminPolicy";
        var userId = Guid.NewGuid().ToString();
        _currentUserService.Setup(x => x.UserId).Returns(userId);
        _identityService.Setup(_ => _.AuthorizeAsync(userId, policyName)).Returns(Task.FromResult(false));
        var authBehavior = new AuthorizationBehavior<AdminPolicyQuery, Unit>(_currentUserService.Object, _identityService.Object);

        // Act
        var request = authBehavior.Handle(new AdminPolicyQuery(), new CancellationToken(), next: null!);

        // Assert
        await FluentActions.Invoking(() => request).Should().ThrowAsync<ForbiddenAccessException>();
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