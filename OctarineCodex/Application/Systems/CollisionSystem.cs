// OctarineCodex/Application/Systems/CollisionSystem.cs

using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Components;
using OctarineCodex.Infrastructure.Ecs;

namespace OctarineCodex.Application.Systems;

/// <summary>
///     Processes collision detection between entities using spatial optimization.
///     Replaces collision logic from EntityWrapper behaviors.
/// </summary>
[Service<CollisionSystem>(ServiceLifetime.Scoped)]
public class CollisionSystem : AEntitySetSystem<float>, ISystem
{
    public CollisionSystem(WorldManager worldManager)
        : base(worldManager.CurrentWorld.GetEntities()
            .With<PositionComponent>()
            .With<CollisionComponent>()
            .AsSet())
    {
    }

    public void Update(GameTime gameTime)
    {
        Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Draw(GameTime gameTime) { } // No drawing for collision system

    protected override void Update(float deltaTime, in Entity entity)
    {
        ref PositionComponent position = ref entity.Get<PositionComponent>();
        ref CollisionComponent collision = ref entity.Get<CollisionComponent>();

        // Update velocity-based movement
        if (!collision.IsStatic && collision.Velocity != Vector2.Zero)
        {
            collision.LastPosition = position.Position;
            position.Position += collision.Velocity * deltaTime;
        }

        // TODO: Add collision detection against other entities
        // This will be expanded with spatial indexing in Phase 4
    }
}
