// OctarineCodex\Application\GameState\GameUpdateManager.cs

using Microsoft.Xna.Framework;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Maps;
using OctarineCodex.Domain.Physics;
using OctarineCodex.Presentation.Input;

namespace OctarineCodex.Application.GameState;

public class GameUpdateManager(
    IInputService inputService,
    IEntityService entityService,
    ICollisionSystem collisionSystem,
    IMapService mapService) : IGameUpdateManager
{
    public bool ShouldExit { get; private set; }

    public void Update(GameTime gameTime)
    {
        // Update input system
        inputService.Update(gameTime);

        // Check for exit input
        if (inputService.IsExitPressed())
        {
            ShouldExit = true;
            return;
        }

        // Only update game systems if world is loaded
        if (!mapService.IsLoaded)
        {
            return;
        }

        // Update all entities (player movement, AI, physics, etc.)
        entityService.Update(gameTime);

        // Process collision system events (triggers, collision messages, etc.)
        collisionSystem.ProcessCollisions();
    }
}
