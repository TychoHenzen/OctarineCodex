// OctarineCodex/Entities/Behaviors/PlayerTeleportBehavior.cs

using System;
using Microsoft.Xna.Framework;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Handles player teleportation between world layers
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 800)]
public class PlayerTeleportBehavior : EntityBehavior
{
    private ICollisionService _collisionService;
    private IEntityService _entityService;
    private IInputService _inputService;
    private ILoggingService _logger;
    private IMapService _mapService;
    private ITeleportService _teleportService;
    private IWorldLayerService _worldLayerService;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        base.Initialize(entity, services);

        _inputService = (IInputService)services.GetService(typeof(IInputService));
        _teleportService = (ITeleportService)services.GetService(typeof(ITeleportService));
        _worldLayerService = (IWorldLayerService)services.GetService(typeof(IWorldLayerService));
        _collisionService = (ICollisionService)services.GetService(typeof(ICollisionService));
        _entityService = (IEntityService)services.GetService(typeof(IEntityService));
        _logger = (ILoggingService)services.GetService(typeof(ILoggingService));
        _mapService = (IMapService)services.GetService(typeof(IMapService));
    }

    public override void Update(GameTime gameTime)
    {
        // Only handle teleportation if multiple levels/layers are available
        if (_mapService.CurrentLevels.Count <= 1)
            return;

        var teleportPressed = _inputService.IsPrimaryActionPressed();
        if (_teleportService.CheckTeleportInteraction(Entity.Position, teleportPressed,
                out var targetDepth, out var targetPos) 
            && _worldLayerService.SwitchToLayer(targetDepth))
        {
            _logger.Debug($"Player teleported to world depth {targetDepth}");

            // Update all systems for new layer FIRST
            var newLayerLevels = _worldLayerService.GetCurrentLayerLevels();
            _collisionService.InitializeCollision(newLayerLevels);
            _entityService.UpdateEntitiesForCurrentLayer(newLayerLevels);
            _teleportService.InitializeTeleports();

            // THEN set the player position after entities are reloaded
            if (targetPos.HasValue)
            {
                var playerEntity = _entityService.GetPlayerEntity();
                if (playerEntity != null)
                {
                    playerEntity.Position = targetPos.Value;
                    _logger.Debug($"Set player position to teleport target: {targetPos.Value}");
                }
            }
        }
    }

    /// <summary>
    ///     Check if teleportation is available at current position
    /// </summary>
    public bool IsTeleportAvailable(out int targetDepth, out Vector2? targetPos)
    {
        return _teleportService.IsTeleportAvailable(Entity.Position, out targetDepth, out targetPos);
    }
}