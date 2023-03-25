using OneTimePassGen.Application.Common.Interfaces;

namespace OneTimePassGen.Infrastructure.Services;

internal sealed class PasswordGeneratorService : IPasswordGenerator
{
    public string GeneratePassword()
    {
        return Guid.NewGuid().ToString();
    }
}