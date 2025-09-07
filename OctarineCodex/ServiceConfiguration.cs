// Updated OctarineCodex/ServiceConfiguration.cs

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;
using OctarineCodex.Messaging;
using OctarineCodex.Services;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex;

/// <summary>
///     Configures dependency injection services for the application.
///     Provides a composition root following SOLID principles.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    ///     Registers all application services with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddOctarineServices(this IServiceCollection services)
    {
        // Logging services
        services.AddSingleton<ILoggingService, LoggingService>();

        // Messaging system services  
        services.AddMessaging();

        // Input system services
        services.AddSingleton<IKeyboardInputProvider, DesktopKeyboardInputProvider>();
        services.AddSingleton<IControllerInputProvider, DesktopControllerInputProvider>();
        services.AddSingleton<IInputService, CompositeInputService>();

        services.AddSingleton<ICameraService>(_ =>
        {
            var worldViewportSize = new Vector2(FixedWidth, FixedHeight) / WorldRenderScale;
            return new CameraService(worldViewportSize);
        });

        services.AddSingleton<IMapService, MapService>();
        services.AddTransient<ILevelRenderer, LevelRenderer>();

        services.AddSingleton<ICollisionService, CollisionService>();
        services.AddSingleton<IWorldLayerService, WorldLayerService>();
        services.AddSingleton<ITeleportService, TeleportService>();

        services.AddSingleton<EntityBehaviorRegistry>();
        services.AddSingleton<IEntityService, EntityService>();

        return services;
    }

    public static void InitializeEntitySystem(IServiceProvider services)
    {
        var registry = services.GetRequiredService<EntityBehaviorRegistry>();
        registry.DiscoverBehaviors(); // Auto-discover all [EntityBehavior] classes

        // Initialize messaging system
        MessagingExtensions.InitializeMessageHandlers(services);
    }
}