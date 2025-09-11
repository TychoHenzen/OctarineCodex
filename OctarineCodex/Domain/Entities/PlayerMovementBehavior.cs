// OctarineCodex/Entities/Behaviors/PlayerMovementBehavior.cs

using Microsoft.Xna.Framework;
using OctarineCodex.Application.Components;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Messages;
using OctarineCodex.Domain.Physics;
using OctarineCodex.Domain.Physics.Shapes;
using OctarineCodex.Presentation.Input;

namespace OctarineCodex.Domain.Entities;

[EntityBehavior(EntityType = "Player", Priority = 1000)]
public class PlayerMovementBehavior(
    IInputService inputService,
    ICollisionSystem collisionSystem)
    : EntityBehavior
{
    private bool _hasCollisionComponent;
    private string _entityId = string.Empty;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity)
    {
        base.Initialize(entity);

        // Convert Guid to string for entity ID
        _entityId = Entity.Iid.ToString();

        // Register player with collision system
        var playerShape = new BoxShape(
            new Rectangle(0, 0, OctarineConstants.PlayerSize, OctarineConstants.PlayerSize));

        var collisionComponent = new CollisionComponent(playerShape, CollisionLayers.Entity)
        {
            CollidesWith = CollisionLayers.Solid | CollisionLayers.Platform, IsStatic = false
        };

        collisionSystem.RegisterEntity(_entityId, collisionComponent, Entity.Position);
        _hasCollisionComponent = true;
    }

    public override void Update(GameTime gameTime)
    {
        if (_hasCollisionComponent)
        {
            EnsureRegisteredWithCollisionSystem();
        }

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 dir = inputService.GetMovementDirection();
        Vector2 delta = ComputeDelta(dir, OctarineConstants.PlayerSpeed, dt);

        if (delta == Vector2.Zero)
        {
            return;
        }

        var previousPos = Entity.Position;
        Vector2 desiredPos = Entity.Position + delta;

        // Use new collision system for movement resolution
        Vector2 correctedPos = collisionSystem.ResolveMovement(_entityId, Entity.Position, desiredPos);

        Entity.Position = correctedPos;
        collisionSystem.UpdateEntityPosition(_entityId, correctedPos);

        // Send movement messages
        if (correctedPos != desiredPos)
        {
            Entity.SendMessage(new MovementBlockedMessage(dir, delta, "Collision"));
        }

        if (correctedPos != previousPos)
        {
            Vector2 actualDelta = correctedPos - previousPos;
            Entity.SendGlobalMessage(new PlayerMovedMessage(correctedPos, actualDelta));
        }
    }

    public override void Cleanup()
    {
        if (_hasCollisionComponent)
        {
            collisionSystem.UnregisterEntity(_entityId);
        }

        base.Cleanup();
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

    private void EnsureRegisteredWithCollisionSystem()
    {
        var playerShape = new BoxShape(
            new Rectangle(0, 0, OctarineConstants.PlayerSize, OctarineConstants.PlayerSize));

        var collisionComponent = new CollisionComponent(playerShape, CollisionLayers.Entity)
        {
            CollidesWith = CollisionLayers.Solid | CollisionLayers.Platform, IsStatic = false
        };

        // Re-register every frame - the collision system will handle this efficiently
        collisionSystem.RegisterEntity(_entityId, collisionComponent, Entity.Position);
    }

}
