using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

namespace OctarineCodex;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Set up dependency injection container
        var services = new ServiceCollection();
        services.AddOctarineServices();

        var serviceProvider = services.BuildServiceProvider();

        // Create game with all injected services
        var logger = serviceProvider.GetRequiredService<ILoggingService>();
        var inputService = serviceProvider.GetRequiredService<IInputService>();
        var worldMapService = serviceProvider.GetRequiredService<IWorldMapService>();
        var collisionService = serviceProvider.GetRequiredService<ICollisionService>();
        var entityService = serviceProvider.GetRequiredService<IEntityService>();
        var worldRenderer = serviceProvider.GetRequiredService<IWorldRenderer>();
        var simpleMapService = serviceProvider.GetRequiredService<ISimpleMapService>();
        var simpleLevelRenderer = serviceProvider.GetRequiredService<ISimpleLevelRenderer>();
        var worldLayerService = serviceProvider.GetRequiredService<IWorldLayerService>();
        var teleportService = serviceProvider.GetRequiredService<ITeleportService>();

        using var game = new OctarineGameHost(
            logger,
            inputService,
            worldMapService,
            collisionService,
            entityService,
            worldRenderer,
            simpleMapService,
            simpleLevelRenderer,
            worldLayerService,
            teleportService);

        game.Window.AllowUserResizing = true;
        game.Run();

        await serviceProvider.DisposeAsync();
    }
}