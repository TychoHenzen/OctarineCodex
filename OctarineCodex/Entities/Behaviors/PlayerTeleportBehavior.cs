// OctarineCodex/Entities/Behaviors/PlayerTeleportBehavior.cs

using System.Collections.Generic;
using JetBrains.Annotations;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Collisions;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;
using OctarineCodex.Messages;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Handles player teleportation between world layers.
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 800)]
[UsedImplicitly]
public class PlayerTeleportBehavior(
    IInputService inputService,
    ITeleportService teleportService,
    IWorldLayerService worldLayerService,
    ICollisionSystem collisionSystem,
    IEntityService entityService,
    ILoggingService logger,
    IMapService mapService)
    : EntityBehavior
{
    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Update(GameTime gameTime)
    {
        // Only handle teleportation if multiple levels/layers are available
        if (mapService.CurrentLevels.Count <= 1)
        {
            return;
        }

        var teleportPressed = inputService.IsPrimaryActionPressed();
        if (!teleportService.CheckTeleportInteraction(
                Entity.Position,
                teleportPressed,
                out var targetDepth,
                out Vector2? targetPos)
            || !worldLayerService.SwitchToLayer(targetDepth))
        {
            return;
        }

        logger.Debug($"Player teleported to world depth {targetDepth}");

        // Update all systems for new layer FIRST
        IReadOnlyList<LDtkLevel> newLayerLevels = worldLayerService.GetCurrentLayerLevels();
        collisionSystem.InitializeLevels(newLayerLevels);
        entityService.UpdateEntitiesForCurrentLayer(newLayerLevels);
        teleportService.InitializeTeleports();

        // THEN set the player position after entities are reloaded
        if (!targetPos.HasValue)
        {
            return;
        }

        Vector2 previousPosition = Entity.Position;
        EntityWrapper playerEntity = entityService.GetPlayerEntity();

        playerEntity.Position = targetPos.Value;
        logger.Debug($"Set player position to teleport target: {targetPos.Value}");

        // Send global PlayerMovedMessage so camera and other systems get notified
        Vector2 teleportDelta = targetPos.Value - previousPosition;
        Entity.SendGlobalMessage(new PlayerMovedMessage(targetPos.Value, teleportDelta));

        logger.Debug($"Sent PlayerMovedMessage for teleport: {previousPosition} -> {targetPos.Value}");
    }

    /// <summary>
    ///     Check if teleportation is available at current position.
    /// </summary>
    public bool IsTeleportAvailable(out int targetDepth, out Vector2? targetPos)
    {
        return teleportService.IsTeleportAvailable(Entity.Position, out targetDepth, out targetPos);
    }
}
