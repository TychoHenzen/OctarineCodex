using Microsoft.Extensions.DependencyInjection;
using OctarineCodex;
using OctarineCodex.Input;

// Set up dependency injection container
var services = new ServiceCollection();
services.AddOctarineServices();

var serviceProvider = services.BuildServiceProvider();

// Create game with injected services
var inputService = serviceProvider.GetRequiredService<IInputService>();
using var game = new Game1(inputService);
game.Run();

await serviceProvider.DisposeAsync();