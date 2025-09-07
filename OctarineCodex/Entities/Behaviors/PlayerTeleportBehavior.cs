// OctarineCodex/Entities/Behaviors/PlayerTeleportBehavior.cs

using System;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities.Messages;
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
    private readonly ICollisionService _collisionService;
    private readonly IEntityService _entityService;
    private readonly IInputService _inputService;
    private readonly ILoggingService _logger;
    private readonly IMapService _mapService;
    private readonly ITeleportService _teleportService;
    private readonly IWorldLayerService _worldLayerService;

    public PlayerTeleportBehavior(
        IInputService inputService,
        ITeleportService teleportService,
        IWorldLayerService worldLayerService,
        ICollisionService collisionService,
        IEntityService entityService,
        ILoggingService logger,
        IMapService mapService)
    {
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _teleportService = teleportService ?? throw new ArgumentNullException(nameof(teleportService));
        _worldLayerService = worldLayerService ?? throw new ArgumentNullException(nameof(worldLayerService));
        _collisionService = collisionService ?? throw new ArgumentNullException(nameof(collisionService));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
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
                var previousPosition = Entity.Position;
                var playerEntity = _entityService.GetPlayerEntity();
                if (playerEntity != null)
                {
                    playerEntity.Position = targetPos.Value;
                    _logger.Debug($"Set player position to teleport target: {targetPos.Value}");

                    // Send global PlayerMovedMessage so camera and other systems get notified
                    var teleportDelta = targetPos.Value - previousPosition;
                    Entity.SendGlobalMessage(new PlayerMovedMessage(targetPos.Value, teleportDelta));

                    _logger.Debug($"Sent PlayerMovedMessage for teleport: {previousPosition} -> {targetPos.Value}");
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