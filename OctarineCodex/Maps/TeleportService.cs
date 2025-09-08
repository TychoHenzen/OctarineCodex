using System;
using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

public class TeleportService : ITeleportService
{
    private readonly IEntityService _entityService;
    private readonly ILoggingService _logger;
    private readonly List<TeleportData> _teleports = new();
    private readonly IWorldLayerService _worldLayerService;

    public TeleportService(IEntityService entityService, IWorldLayerService worldLayerService, ILoggingService logger)
    {
        _entityService = entityService;
        _worldLayerService = worldLayerService;
        _logger = logger;
    }

    public void InitializeTeleports()
    {
        _teleports.Clear();

        // Get all entity wrappers to check for teleport-like entities
        var allEntities = _entityService.GetAllEntities();
        _logger.Debug($"TeleportService: Checking {allEntities.Count()} total entities for teleports");

        var teleportLikeEntities = allEntities.Where(e =>
            e.EntityType.ToLowerInvariant().Contains("teleport")).ToList();

        _logger.Debug(
            $"Found {teleportLikeEntities.Count} teleport-like entities: {string.Join(", ", teleportLikeEntities.Select(e => $"{e.EntityType}@{e.Position}"))}");

        foreach (var entity in teleportLikeEntities)
        {
            // Check if entity has a destination field
            if (entity.HasField("destination"))
            {
                try
                {
                    var destination = entity.GetField<EntityReference>("destination");
                    if (destination != null)
                    {
                        var destinationInfo = ResolveDestination(destination);
                        if (destinationInfo.HasValue)
                        {
                            _teleports.Add(new TeleportData
                            {
                                Position = entity.Position,
                                Size = entity.Size,
                                TargetWorldDepth = destinationInfo.Value.WorldDepth,
                                TargetPosition = destinationInfo.Value.Position
                            });
                        }
                        else
                            _logger.Warn($"Could not resolve destination for teleport at {entity.Position}");
                    }
                    else
                    {
                        _logger.Debug($"Teleport at {entity.Position} has null destination");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to get destination field for teleport at {entity.Position}: {ex.Message}");
                }
            }
            else
            {
                _logger.Debug(
                    $"Teleport-like entity {entity.EntityType} at {entity.Position} has no destination field");
            }
        }

        _logger.Debug(
            $"Initialized {_teleports.Count} teleports for world layer {_worldLayerService.CurrentWorldDepth}");
    }

    public bool IsTeleportAvailable(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition)
    {
        return CheckTeleportProximity(playerPosition, out targetWorldDepth, out targetPosition);
    }

    public bool CheckTeleportInteraction(Vector2 playerPosition, bool inputPressed, out int targetWorldDepth,
        out Vector2? targetPosition)
    {
        // Check if player is in range of any teleport
        var inRange = CheckTeleportProximity(playerPosition, out targetWorldDepth, out targetPosition);

        if (inRange && inputPressed)
        {
            _logger.Debug($"Player activated teleport to world depth {targetWorldDepth} at {targetPosition}");
            return true;
        }

        targetWorldDepth = _worldLayerService.CurrentWorldDepth;
        targetPosition = null;
        return false;
    }

    private (Vector2 Position, int WorldDepth)? ResolveDestination(EntityReference entityRef)
    {
        // Find the target level using the LevelIid
        var allLevels = _worldLayerService.GetAllLevels();
        var targetLevel = allLevels.FirstOrDefault(l => l.Iid == entityRef.LevelIid);

        if (targetLevel == null)
        {
            _logger.Warn($"Could not find target level with IID {entityRef.LevelIid}");
            return null;
        }

        // Find the target entity within the level
        var targetEntity = FindEntityInLevel(targetLevel, entityRef.EntityIid);
        if (targetEntity == null)
        {
            _logger.Warn(
                $"Could not find target entity with IID {entityRef.EntityIid} in level {targetLevel.Identifier}");
            return null;
        }

        var worldPosition = new Vector2(
            targetLevel.WorldX + targetEntity.Px.X,
            targetLevel.WorldY + targetEntity.Px.Y
        );

        return (worldPosition, targetLevel.WorldDepth);
    }

    private static EntityInstance? FindEntityInLevel(LDtkLevel level, Guid entityIid)
    {
        if (level.LayerInstances == null)
        {
            return null;
        }

        foreach (var layer in level.LayerInstances.Where(l => l._Type == LayerType.Entities))
        {
            var entity = layer.EntityInstances.FirstOrDefault(e => e.Iid == entityIid);
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    private bool CheckTeleportProximity(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition)
    {
        targetWorldDepth = _worldLayerService.CurrentWorldDepth;
        targetPosition = null;

        const float interactionDistance = 48f; // Increased from 16 to 48 pixels for easier interaction

        foreach (var teleport in _teleports)
        {
            var distance = Vector2.Distance(playerPosition, teleport.Position);

            // Debug log every few frames to see proximity

            if (distance <= interactionDistance)
            {
                targetWorldDepth = teleport.TargetWorldDepth;
                targetPosition = teleport.TargetPosition;
                return true;
            }
        }

        return false;
    }

    private sealed class TeleportData
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public int TargetWorldDepth { get; set; }
        public Vector2 TargetPosition { get; set; }
    }
}