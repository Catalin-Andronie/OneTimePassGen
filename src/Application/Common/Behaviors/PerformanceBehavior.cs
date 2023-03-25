using System.Diagnostics;

using MediatR;

using Microsoft.Extensions.Logging;

using OneTimePassGen.Application.Common.Interfaces;

namespace OneTimePassGen.Application.Common.Behaviors;

internal sealed class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly IApplicationConfiguration _configuration;

    public PerformanceBehavior(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IApplicationConfiguration configuration)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _configuration = configuration;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _timer.Start();

        TResponse? response = await next().ConfigureAwait(false);

        _timer.Stop();

        long elapsedMilliseconds = _timer.ElapsedMilliseconds;

        double longRunningRequestLimitMilliseconds = _configuration.GetLongRunningRequestLimitMilliseconds();

        if (elapsedMilliseconds >= longRunningRequestLimitMilliseconds)
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

            _logger.LogWarning(
                message: "Long Running Request: '{@RequestName}' took ({@ElapsedMilliseconds} ms) and limit is ({@LongRunningRequestLimitMilliseconds} ms). Request made by user `{@UserName}` with identifier '{@UserId}'.",
                requestName, elapsedMilliseconds, longRunningRequestLimitMilliseconds, userName, userId);
        }

        return response;
    }
}