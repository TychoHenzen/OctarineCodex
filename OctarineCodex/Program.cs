using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;
using OctarineCodex.Services;

namespace OctarineCodex;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddOctarineServices();

        var serviceProvider = services.BuildServiceProvider();


        // Create game with all injected services
        var logger = serviceProvider.GetRequiredService<ILoggingService>();
        var inputService = serviceProvider.GetRequiredService<IInputService>();
        var collisionService = serviceProvider.GetRequiredService<ICollisionService>();
        var entityService = serviceProvider.GetRequiredService<IEntityService>();
        var mapService = serviceProvider.GetRequiredService<IMapService>();
        var levelRenderer = serviceProvider.GetRequiredService<ILevelRenderer>();
        var worldLayerService = serviceProvider.GetRequiredService<IWorldLayerService>();
        var teleportService = serviceProvider.GetRequiredService<ITeleportService>();
        var cameraService = serviceProvider.GetRequiredService<ICameraService>();

        using var game = new OctarineGameHost(
            logger,
            inputService,
            mapService,
            levelRenderer,
            collisionService,
            entityService,
            worldLayerService,
            teleportService,
            cameraService);
        ServiceConfiguration.InitializeEntitySystem(serviceProvider);

        game.Window.AllowUserResizing = true;
        game.Run();

        await serviceProvider.DisposeAsync();
    }
}