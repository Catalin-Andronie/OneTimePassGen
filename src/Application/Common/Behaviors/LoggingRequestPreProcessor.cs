﻿using MediatR.Pipeline;

using Microsoft.Extensions.Logging;

using OneTimePassGen.Application.Common.Interfaces;

namespace OneTimePassGen.Application.Common.Behaviors;

public sealed class LoggingRequestPreProcessor<TRequest>
    : IRequestPreProcessor<TRequest>
        where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public LoggingRequestPreProcessor(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string userId = "Unknown";
        string userName = "Anonymous";

        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            userId = _currentUserService.UserId;
            userName = await _identityService
                .GetUserNameAsync(userId)
                .ConfigureAwait(false) ?? userName;
        }

        _logger.LogInformation("Request: '{RequestName}' made by user `{@UserName}` with identifier `{@UserId}`.", requestName, userName, userId);
    }
}