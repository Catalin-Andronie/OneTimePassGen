using Microsoft.Extensions.Configuration;

using OneTimePassGen.Application.Common.Interfaces;

namespace OneTimePassGen.Infrastructure;

internal sealed class ApplicationConfiguration : IApplicationConfiguration
{
    private readonly IConfiguration _configuration;

    public ApplicationConfiguration(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public object GetValue(Type type, string key) => _configuration.GetValue(type, key);

    public T GetValue<T>(string key) => _configuration.GetValue<T>(key);
}