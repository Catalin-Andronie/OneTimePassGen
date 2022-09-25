using OneTimePassGen.Application.Common.Interfaces;

namespace OneTimePassGen.Infrastructure.Services;

internal sealed class DateTimeService : IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.Now;

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}