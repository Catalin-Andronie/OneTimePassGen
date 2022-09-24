namespace OneTimePassGen.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
}