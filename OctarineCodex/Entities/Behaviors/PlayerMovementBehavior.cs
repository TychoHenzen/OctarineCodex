// OctarineCodex/Entities/Behaviors/PlayerMovementBehavior.cs

using Microsoft.Xna.Framework;
using OctarineCodex.Entities.Messages;
using OctarineCodex.Input;
using OctarineCodex.Maps;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Handles player movement input and physics using existing Movement logic.
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 1000)]
public class PlayerMovementBehavior(
    IInputService inputService,
    ICollisionService collisionService,
    IMapService mapService)
    : EntityBehavior
{
    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 dir = inputService.GetMovementDirection();
        Vector2 delta = ComputeDelta(dir, OctarineConstants.PlayerSpeed, dt);

        if (delta == Vector2.Zero)
        {
            return;
        }

        // Calculate new position
        var newPos = Entity.Position + delta;
        var previousPos = Entity.Position;

        // Handle collision resolution
        Vector2 correctedPos;
        if (mapService.CurrentLevels.Count > 1)
        {
            // Multi-level world: use collision service
            correctedPos = collisionService.ResolveCollision(
                Entity.Position, newPos, new Vector2(OctarineConstants.PlayerSize, OctarineConstants.PlayerSize));
        }
        else
        {
            // Single level: simple bounds checking
            Rectangle bounds = mapService.GetWorldBounds();
            correctedPos = new Vector2(
                MathHelper.Clamp(newPos.X, bounds.X, bounds.X + bounds.Width - OctarineConstants.PlayerSize),
                MathHelper.Clamp(newPos.Y, bounds.Y, bounds.Y + bounds.Height - OctarineConstants.PlayerSize));
        }

        // Update entity position
        Entity.Position = correctedPos;

        // Send movement messages
        if (correctedPos != newPos)
        {
            Entity.SendMessage(new MovementBlockedMessage(dir, delta));
        }

        if (correctedPos == previousPos)
        {
            return;
        }

        Vector2 actualDelta = correctedPos - previousPos;
        Entity.SendGlobalMessage(new PlayerMovedMessage(correctedPos, actualDelta));
    }

    private static Vector2 ComputeDelta(Vector2 direction, float speed, float dt)
    {
        if (direction == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        direction.Normalize();
        return direction * speed * dt;
    }
}
