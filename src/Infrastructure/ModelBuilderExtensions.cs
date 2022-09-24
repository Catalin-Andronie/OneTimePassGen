using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OneTimePassGen.Infrastructure;

/// <summary>
///     Extension points over <see cref="ModelBuilder"/>.
/// </summary>
internal static class ModelBuilderExtensions
{
    /// <summary>
    ///      Allows registration of arbitrary <see cref="ValueConvertor"/>(s) for the specified <typeparamref name="T"/> type.
    ///      Read more: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
    /// </summary>
    /// <typeparam name="T">The type for which the convertor to be applied.</typeparam>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
    /// <param name="converter">The <see cref="ValueConvertor"/> instance.</param>
    /// <returns>Returns <see cref="ModelBuilder"/> to allow method chaining.</returns>
    public static ModelBuilder UseValueConverterForType<T>(this ModelBuilder modelBuilder, ValueConverter converter)
    {
        return modelBuilder.UseValueConverterForType(typeof(T), converter);
    }

    /// <summary>
    ///      Allows registration of arbitrary <see cref="ValueConvertor"/>(s) for the specified <paramref name="type"/>.
    ///      Read more: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
    /// <param name="type">The type for which the convertor to be applied.</param>
    /// <param name="converter">The <see cref="ValueConvertor"/> instance.</param>
    /// <returns>Returns <see cref="ModelBuilder"/> to allow method chaining.</returns>
    public static ModelBuilder UseValueConverterForType(this ModelBuilder modelBuilder, Type type, ValueConverter converter)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // note that entityType.GetProperties() will throw an exception, so we have to use reflection 
            var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);
            foreach (var property in properties)
            {
                modelBuilder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(converter);
            }
        }

        return modelBuilder;
    }
}