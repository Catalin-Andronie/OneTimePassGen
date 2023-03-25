using OneTimePassGen.Application.Common.Interfaces;

namespace OneTimePassGen.Application;

internal static class ApplicationConfigurationExtensions
{
    public static double GetLongRunningRequestLimitMilliseconds(this IApplicationConfiguration appConfig)
    {
        const string configurationKey = "Application:LongRunningRequestLimitMilliseconds";
        return GetRequiredValue<double>(appConfig, configurationKey);
    }

    public static int GetGeneratedPasswordExpirationSeconds(this IApplicationConfiguration appConfig)
    {
        const string configurationKey = "Application:GeneratedPasswordExpirationSeconds";
        return GetRequiredValue<int>(appConfig, configurationKey);
    }

    private static T GetRequiredValue<T>(this IApplicationConfiguration appConfig, string configurationKey)
    {
        T? value = appConfig.GetValue<T?>(configurationKey);
        return value ?? throw new KeyNotFoundException($"Configuration key '{configurationKey}' doesn't exist or has a null value.");
    }
}
