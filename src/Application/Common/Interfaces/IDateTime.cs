namespace OneTimePassGen.Application.Common.Interfaces;

public interface IDateTime
{
    DateTimeOffset Now { get; }

    DateTimeOffset UtcNow { get; }
}