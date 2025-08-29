using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Input;
using OctarineCodex.Maps;

namespace OctarineCodex;

/// <summary>
/// Configures dependency injection services for the application.
/// Provides a composition root following SOLID principles.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers all application services with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddOctarineServices(this IServiceCollection services)
    {
        // Input system services
        services.AddSingleton<IKeyboardInputProvider, DesktopKeyboardInputProvider>();
        services.AddSingleton<IControllerInputProvider, DesktopControllerInputProvider>();
        services.AddSingleton<IInputService, CompositeInputService>();

        // LDTK map system services
        services.AddSingleton<ILdtkMapService, LdtkMonoGameMapService>();
        services.AddTransient<ILdtkMapRenderer, LdtkMapRenderer>();

        return services;
    }
}