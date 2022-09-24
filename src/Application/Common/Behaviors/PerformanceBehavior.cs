﻿using System.Diagnostics;

using OneTimePassGen.Application.Common.Interfaces;

using MediatR;

using Microsoft.Extensions.Logging;

namespace OneTimePassGen.Application.Common.Behaviors;

internal sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private const double LongRunningRequestElapsedMilliseconds = 500;

    public PerformanceBehavior(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _timer.Start();

        var response = await next().ConfigureAwait(false);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds >= LongRunningRequestElapsedMilliseconds)
        {
            var requestName = typeof(TRequest).Name;
            var userId = "Unknown";
            string userName = "Anonymous";

            if (!string.IsNullOrEmpty(_currentUserService.UserId))
            {
                userId = _currentUserService.UserId;
                userName = await _identityService.GetUserNameAsync(userId).ConfigureAwait(false);
            }

            _logger.LogWarning(
                "Long Running Request: '{@RequestName}' took ({@ElapsedMilliseconds} milliseconds) made by user `{@UserName}` with identifier '{@UserId}'.",
                requestName, elapsedMilliseconds, userName, userId);
        }

        return response;
    }
}