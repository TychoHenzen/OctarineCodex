using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OctarineCodex;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddOctarineServices();

        var serviceProvider = services.BuildServiceProvider();

        // Initialize systems that require post-registration setup
        ServiceConfiguration.InitializeGameSystems(serviceProvider);

        // Get the game host from DI instead of manual construction
        using var game = serviceProvider.GetRequiredService<OctarineGameHost>();
        game.init();
        game.Window.AllowUserResizing = true;
        game.Run();

        await serviceProvider.DisposeAsync().ConfigureAwait(false);
    }
}
