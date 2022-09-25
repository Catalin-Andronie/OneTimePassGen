namespace OneTimePassGen.Application.Common.Interfaces;

public interface IApplicationConfiguration
{
    object GetValue(Type type, string key);
    T GetValue<T>(string key);
}