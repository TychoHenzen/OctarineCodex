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
    private ICollisionService _collisionService;

    private IInputService _inputService;
    private IMapService _mapService;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        base.Initialize(entity, services);

        _inputService = (IInputService)services.GetService(typeof(IInputService));
        _collisionService = (ICollisionService)services.GetService(typeof(ICollisionService));
        _mapService = (IMapService)services.GetService(typeof(IMapService));
    }

    public override void Update(GameTime gameTime)
    {
        if (_inputService == null) return;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();
        var delta = ComputeDelta(dir, OctarineConstants.PlayerSpeed, dt);

        if (delta == Vector2.Zero) return;

        // Calculate new position
        var newPos = Entity.Position + delta;

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

        // Send movement message for other behaviors to react
        if (correctedPos != Entity.Position + delta)
            Entity.SendMessage(new MovementBlockedMessage(dir, delta));
        else
            Entity.SendMessage(new PlayerMovedMessage(Entity.Position, correctedPos));
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