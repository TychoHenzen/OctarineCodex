using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;
using Room4;

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

        // Use the generated Teleport entities instead of manual parsing
        var teleportEntities = _entityService.GetGeneratedEntitiesOfType<Teleport>();
        
        foreach (var teleport in teleportEntities)
        {
            if (teleport.destination != null)
            {
                // Resolve the destination using the EntityReference
                var destinationInfo = ResolveDestination(teleport.destination);
                
                if (destinationInfo.HasValue)
                {
                    _logger.Debug($"Found teleport at {teleport.Position} targeting entity at {destinationInfo.Value.Position} on world depth {destinationInfo.Value.WorldDepth}");

                    _teleports.Add(new TeleportData
                    {
                        Position = teleport.Position,
                        Size = teleport.Size,
                        TargetWorldDepth = destinationInfo.Value.WorldDepth,
                        TargetPosition = destinationInfo.Value.Position
                    });
                }
                else
                {
                    _logger.Warn($"Could not resolve destination for teleport at {teleport.Position}");
                }
            }
            else
            {
                _logger.Debug($"Teleport at {teleport.Position} has no destination configured");
            }
        }

        _logger.Debug($"Initialized {_teleports.Count} teleports for world layer {_worldLayerService.CurrentWorldDepth}");
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
            _logger.Warn($"Could not find target entity with IID {entityRef.EntityIid} in level {targetLevel.Identifier}");
            return null;
        }

        var worldPosition = new Vector2(
            targetLevel.WorldX + targetEntity.Px.X,
            targetLevel.WorldY + targetEntity.Px.Y
        );

        return (worldPosition, targetLevel.WorldDepth);
    }

    private EntityInstance? FindEntityInLevel(LDtkLevel level, System.Guid entityIid)
    {
        if (level.LayerInstances == null) return null;

        foreach (var layer in level.LayerInstances.Where(l => l._Type == LayerType.Entities))
        {
            var entity = layer.EntityInstances.FirstOrDefault(e => e.Iid == entityIid);
            if (entity != null) return entity;
        }

        return null;
    }

    public bool IsTeleportAvailable(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition)
    {
        return CheckTeleportProximity(playerPosition, out targetWorldDepth, out targetPosition);
    }

    public bool CheckTeleportInteraction(Vector2 playerPosition, bool inputPressed, out int targetWorldDepth, out Vector2? targetPosition)
    {
        // Only allow teleportation if player is in range AND pressed the action button
        if (inputPressed && CheckTeleportProximity(playerPosition, out targetWorldDepth, out targetPosition))
        {
            _logger.Debug($"Player activated teleport to world depth {targetWorldDepth}");
            return true;
        }

        targetWorldDepth = _worldLayerService.CurrentWorldDepth;
        targetPosition = null;
        return false;
    }

    private bool CheckTeleportProximity(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition)
    {
        targetWorldDepth = _worldLayerService.CurrentWorldDepth;
        targetPosition = null;

        const float interactionDistance = 16f; // pixels

        foreach (var teleport in _teleports)
        {
            var distance = Vector2.Distance(playerPosition, teleport.Position);
            if (distance <= interactionDistance)
            {
                targetWorldDepth = teleport.TargetWorldDepth;
                targetPosition = teleport.TargetPosition;
                return true;
            }
        }

        return false;
    }

    private class TeleportData
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public int TargetWorldDepth { get; set; }
        public Vector2 TargetPosition { get; set; } // Now required, not nullable
    }
}