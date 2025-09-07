// OctarineCodex/Entities/Behaviors/PlayerMovementBehavior.cs

using System;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities.Messages;
using OctarineCodex.Input;
using OctarineCodex.Maps;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Handles player movement input and physics using existing Movement logic
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 1000)]
public class PlayerMovementBehavior : EntityBehavior
{
    private readonly ICollisionService _collisionService;
    private readonly IInputService _inputService;
    private readonly IMapService _mapService;

    public PlayerMovementBehavior(IInputService inputService, ICollisionService collisionService,
        IMapService mapService)
    {
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _collisionService = collisionService ?? throw new ArgumentNullException(nameof(collisionService));
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();
        var delta = ComputeDelta(dir, OctarineConstants.PlayerSpeed, dt);

        if (delta == Vector2.Zero) return;

        // Calculate new position
        var newPos = Entity.Position + delta;
        var previousPos = Entity.Position;

        // Handle collision resolution
        Vector2 correctedPos;
        if (_mapService.CurrentLevels.Count > 1)
        {
            // Multi-level world: use collision service
            correctedPos = _collisionService.ResolveCollision(
                Entity.Position, newPos, new Vector2(OctarineConstants.PlayerSize, OctarineConstants.PlayerSize));
        }
        else
        {
            // Single level: simple bounds checking  
            var bounds = _mapService.GetWorldBounds();
            correctedPos = new Vector2(
                MathHelper.Clamp(newPos.X, bounds.X, bounds.X + bounds.Width - OctarineConstants.PlayerSize),
                MathHelper.Clamp(newPos.Y, bounds.Y, bounds.Y + bounds.Height - OctarineConstants.PlayerSize)
            );
        }

        // Update entity position
        Entity.Position = correctedPos;

        // Send movement messages
        if (correctedPos != newPos)
            Entity.SendMessage(new MovementBlockedMessage(dir, delta));
        if (correctedPos != previousPos)
        {
            var actualDelta = correctedPos - previousPos;
            Entity.SendGlobalMessage(new PlayerMovedMessage(correctedPos, actualDelta));
        }
    }

    public static Vector2 ComputeDelta(Vector2 direction, float speed, float dt)
    {
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
            return direction * speed * dt;
        }

        return Vector2.Zero;
    }
}