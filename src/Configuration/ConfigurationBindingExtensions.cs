using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mazilious.Common.Configuration;

/// <summary>
/// Extension methods for binding configuration to options.
/// </summary>
public static class ConfigurationBindingExtensions
{
    /// <summary>
    /// Binds a configuration section to an options class.
    /// </summary>
    /// <typeparam name="TOptions">The options class type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection BindFromConfiguration<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration
    )
        where TOptions : class
    {
        services.Configure<TOptions>(configuration.GetSection(typeof(TOptions).Name));
        return services;
    }
}