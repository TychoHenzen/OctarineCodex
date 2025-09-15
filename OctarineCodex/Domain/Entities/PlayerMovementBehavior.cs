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
    private string _entityId = string.Empty;
    private bool _hasCollisionComponent;
    private Vector2 _previousInput = Vector2.Zero;
    private bool _wasMovingLastFrame;

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
        Vector2 currentInput = inputService.GetMovementDirection();
        Vector2 delta = ComputeDelta(currentInput, OctarineConstants.PlayerSpeed, dt);

        var previousPos = Entity.Position;
        var hasInput = currentInput != Vector2.Zero;
        var inputChanged = currentInput != _previousInput;

        // Handle movement when there's input
        if (hasInput)
        {
            Vector2 desiredPos = Entity.Position + delta;
            Vector2 correctedPos = collisionSystem.ResolveMovement(_entityId, Entity.Position, desiredPos);

            Entity.Position = correctedPos;
            collisionSystem.UpdateEntityPosition(_entityId, correctedPos);

            // Check if movement was successful or blocked
            if (correctedPos != desiredPos)
            {
                // Movement was blocked - player is pushing against something
                Entity.SendMessage(new MovementBlockedMessage(currentInput, delta, "Collision"));
                _wasMovingLastFrame = false;
            }
            else if (correctedPos != previousPos)
            {
                // Successful movement
                Vector2 actualDelta = correctedPos - previousPos;
                Entity.SendGlobalMessage(new PlayerMovedMessage(correctedPos, actualDelta));
                _wasMovingLastFrame = true;
            }
        }
        else
        {
            // No input - send idle message if we were previously moving or input just stopped
            if (_wasMovingLastFrame || inputChanged)
            {
                Entity.SendMessage(new PlayerIdleMessage());
                _wasMovingLastFrame = false;
            }
        }

        _previousInput = currentInput;
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

        collisionSystem.RegisterEntity(_entityId, collisionComponent, Entity.Position);
    }
}
