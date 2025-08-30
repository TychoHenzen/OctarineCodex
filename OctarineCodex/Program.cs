using Microsoft.Extensions.DependencyInjection;
using OctarineCodex;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

// Set up dependency injection container
var services = new ServiceCollection();
services.AddOctarineServices();

var serviceProvider = services.BuildServiceProvider();

// Create game with injected services
var logger = serviceProvider.GetRequiredService<ILoggingService>();
var inputService = serviceProvider.GetRequiredService<IInputService>();
var mapService = serviceProvider.GetRequiredService<ISimpleMapService>();
var mapRenderer = serviceProvider.GetRequiredService<ISimpleLevelRenderer>();
using var game = new OctarineGameHost(logger, inputService, mapService, mapRenderer);
game.Run();

await serviceProvider.DisposeAsync();