// OctarineCodex/ServiceConfiguration.cs

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities;
using OctarineCodex.Messaging;
using OctarineCodex.Services;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex;

/// <summary>
///     Configures dependency injection services for the application.
///     Uses automatic service discovery via reflection for most services.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    ///     Registers all application services with the DI container.
    ///     Most services are auto-discovered via [Service&lt;TImplementation&gt;] attributes.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddOctarineServices(this IServiceCollection services)
    {
        // Auto-register all services marked with [Service<TImplementation>] attributes
        services.AddServicesFromAssembly();

        // Manual registration for services that need custom configuration
        services.AddCustomServices();

        return services;
    }

    /// <summary>
    ///     Register services that require custom configuration and cannot be auto-discovered.
    /// </summary>
    private static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Game host
        services.AddSingleton<OctarineGameHost>();

        // Entity behavior registry (no interface, just concrete class)
        services.AddSingleton<EntityBehaviorRegistry>();

        return services;
    }

    /// <summary>
    ///     Initialize systems that require post-registration setup.
    /// </summary>
    public static void InitializeGameSystems(IServiceProvider services)
    {
        var registry = services.GetRequiredService<EntityBehaviorRegistry>();
        registry.DiscoverBehaviors(); // Auto-discover all [EntityBehavior] classes

        // Initialize messaging system
        MessagingExtensions.InitializeMessageHandlers(services);
    }
}