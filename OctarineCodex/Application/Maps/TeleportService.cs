using System;
using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Entities;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Application.Maps;

public class TeleportService(IEntityService entityService, IWorldLayerService worldLayerService, ILoggingService logger)
    : ITeleportService
{
    private readonly List<TeleportData> _teleports = [];

    public void InitializeTeleports()
    {
        _teleports.Clear();

        // Get all entity wrappers to check for teleport-like entities
        List<EntityWrapper> allEntities = entityService.GetAllEntities().ToList();
        logger.Debug($"TeleportService: Checking {allEntities.Count} total entities for teleports");

        var teleportLikeEntities = allEntities.Where(e =>
            e.EntityType.Contains("teleport", StringComparison.InvariantCultureIgnoreCase)).ToList();

        logger.Debug(
            $"Found {teleportLikeEntities.Count} teleport-like entities: {string.Join(", ", teleportLikeEntities.Select(e => $"{e.EntityType}@{e.Position}"))}");

        foreach (var entity in teleportLikeEntities)
        {
            // Check if entity has a destination field
            if (!entity.HasField("destination"))
            {
                logger.Debug(
                    $"Teleport-like entity {entity.EntityType} at {entity.Position} has no destination field");
                return;
            }

            try
            {
                var destination = entity.GetField<EntityReference>("destination");
                (Vector2 Position, int WorldDepth)? destinationInfo = ResolveDestination(destination);
                if (!destinationInfo.HasValue)
                {
                    logger.Warn($"Could not resolve destination for teleport at {entity.Position}");
                    return;
                }

                _teleports.Add(new TeleportData
                {
                    Position = entity.Position,
                    Size = entity.Size,
                    TargetWorldDepth = destinationInfo.Value.WorldDepth,
                    TargetPosition = destinationInfo.Value.Position
                });
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get destination field for teleport at {entity.Position}: {ex.Message}");
            }
        }

        logger.Debug(
            $"Initialized {_teleports.Count} teleports for world layer {worldLayerService.CurrentWorldDepth}");
    }

    public bool IsTeleportAvailable(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition)
    {
        return CheckTeleportProximity(playerPosition, out targetWorldDepth, out targetPosition);
    }

    public bool CheckTeleportInteraction(
        Vector2 playerPosition,
        bool inputPressed,
        out int targetWorldDepth,
        out Vector2? targetPosition)
    {
        // Check if player is in range of any teleport
        var inRange = CheckTeleportProximity(playerPosition, out targetWorldDepth, out targetPosition);

        if (inRange && inputPressed)
        {
            logger.Debug($"Player activated teleport to world depth {targetWorldDepth} at {targetPosition}");
            return true;
        }

        targetWorldDepth = worldLayerService.CurrentWorldDepth;
        targetPosition = null;
        return false;
    }

    private static EntityInstance? FindEntityInLevel(LDtkLevel level, Guid entityIid)
    {
        if (level.LayerInstances == null)
        {
            return null;
        }

        foreach (LayerInstance? layer in level.LayerInstances.Where(l => l._Type == LayerType.Entities))
        {
            EntityInstance? entity = layer.EntityInstances.FirstOrDefault(e => e.Iid == entityIid);
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    private (Vector2 Position, int WorldDepth)? ResolveDestination(EntityReference entityRef)
    {
        // Find the target level using the LevelIid
        IReadOnlyList<LDtkLevel> allLevels = worldLayerService.GetAllLevels();
        var targetLevel = allLevels.FirstOrDefault(l => l.Iid == entityRef.LevelIid);

        if (targetLevel == null)
        {
            logger.Warn($"Could not find target level with IID {entityRef.LevelIid}");
            return null;
        }

        // Find the target entity within the level
        var targetEntity = FindEntityInLevel(targetLevel, entityRef.EntityIid);
        if (targetEntity == null)
        {
            logger.Warn(
                $"Could not find target entity with IID {entityRef.EntityIid} in level {targetLevel.Identifier}");
            return null;
        }

        var worldPosition = new Vector2(
            targetLevel.WorldX + targetEntity.Px.X,
            targetLevel.WorldY + targetEntity.Px.Y);

        return (worldPosition, targetLevel.WorldDepth);
    }

    private bool CheckTeleportProximity(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition)
    {
        targetWorldDepth = worldLayerService.CurrentWorldDepth;
        targetPosition = null;

        const float interactionDistance = 16f; // Increased from 16 to 48 pixels for easier interaction

        TeleportData? teleport = _teleports
            .Select(teleport =>
                new { teleport, distance = Vector2.Distance(playerPosition, teleport.Position) })
            .Where(t => t.distance <= interactionDistance)
            .Select(t => t.teleport).FirstOrDefault();
        if (teleport == null)
        {
            return false;
        }

        targetWorldDepth = teleport.TargetWorldDepth;
        targetPosition = teleport.TargetPosition;
        return true;
    }

    private sealed class TeleportData
    {
        public Vector2 Position { get; init; }
        public Vector2 Size { get; set; }
        public int TargetWorldDepth { get; init; }
        public Vector2 TargetPosition { get; init; }
    }
}
