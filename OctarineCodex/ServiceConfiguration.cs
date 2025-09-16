// OctarineCodex/ServiceConfiguration.cs

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Content;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Application.Services;
using OctarineCodex.Application.Systems;
using OctarineCodex.Infrastructure.Ecs;

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
    /// <param name="gameHost">The game host instance to get ContentManager from.</param>
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

        // Register ContentManager factory - will be resolved when Game.Content is available
        services.AddSingleton<ContentManager>(provider =>
        {
            var gameHost = provider.GetRequiredService<OctarineGameHost>();
            return gameHost.Content;
        });

        // Entity behavior registry (no interface, just concrete class)
        services.AddSingleton<EntityBehaviorRegistry>();

        services.AddSingleton<WorldManager>();
        services.AddSingleton<SystemManager>();
        services.AddSingleton<ComponentRegistry>();

        // ECS Systems
        services.AddScoped<RenderSystem>();
        return services;
    }

    public static void InitializeGameSystems(IServiceProvider services)
    {
        var registry = services.GetRequiredService<EntityBehaviorRegistry>();
        registry.DiscoverBehaviors(); // Auto-discover all [EntityBehavior] classes

        // Initialize messaging system
        MessagingExtensions.InitializeMessageHandlers(services);

        // *** ADD ECS INITIALIZATION ***
        // Initialize ECS world and systems
        var worldManager = services.GetRequiredService<WorldManager>();
        worldManager.Initialize();

        var componentRegistry = services.GetRequiredService<ComponentRegistry>();
        componentRegistry.DiscoverComponents();

        // Register basic systems
        var systemManager = services.GetRequiredService<SystemManager>();
        systemManager.RegisterDrawSystem<RenderSystem>();
    }
}
