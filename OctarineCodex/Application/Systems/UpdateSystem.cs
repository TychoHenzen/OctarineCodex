// OctarineCodex/Application/Systems/UpdateSystem.cs

using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Components;
using OctarineCodex.Infrastructure.Ecs;

namespace OctarineCodex.Application.Systems;

/// <summary>
///     Handles position updates and basic entity logic.
///     Processes movement and transformation for ECS entities.
/// </summary>
[Service<UpdateSystem>(ServiceLifetime.Scoped)]
public class UpdateSystem : AEntitySetSystem<float>, ISystem
{
    public UpdateSystem(WorldManager worldManager)
        : base(worldManager.CurrentWorld.GetEntities()
            .With<PositionComponent>()
            .AsSet())
    {
    }

    public void Update(GameTime gameTime)
    {
        Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Draw(GameTime gameTime) { } // No drawing for update system

    protected override void Update(float deltaTime, in Entity entity)
    {
        // Basic position updates will be handled here
        // For Phase 2, this is mostly a placeholder
        // More complex update logic will be added in later phases
    }
}
