using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using OneTimePassGen.Application.Common.Behaviors;
using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPasswords;

namespace OneTimePassGen.Application.UnitTests.Common.Behaviors;

public sealed class LoggingRequestPreProcessorTests
{
    private Mock<ILogger<GetUserGeneratedPasswordsQuery>> _logger = null!;
    private Mock<ICurrentUserService> _currentUserService = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GetUserGeneratedPasswordsQuery>>();
        _currentUserService = new Mock<ICurrentUserService>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task LoggingRequestPreProcessor_ShouldCall_GetUserNameAsync_OnceIfAuthenticated()
    {
        _currentUserService
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        LoggingRequestPreProcessor<GetUserGeneratedPasswordsQuery> requestLogger
            = new(_logger.Object, _currentUserService.Object, _identityService.Object);

        GetUserGeneratedPasswordsQuery request = new(includeExpiredPasswords: false);

        await requestLogger.Process(request, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task LoggingRequestPreProcessor_ShouldNotCall_GetUserNameAsync_OnceIfUnauthenticated()
    {
        LoggingRequestPreProcessor<GetUserGeneratedPasswordsQuery> requestLogger
            = new(_logger.Object, _currentUserService.Object, _identityService.Object);

        GetUserGeneratedPasswordsQuery request = new(includeExpiredPasswords: false);

        await requestLogger.Process(request, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}