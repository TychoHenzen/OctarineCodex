// OctarineCodex/Entities/Behaviors/CameraFollowBehavior.cs

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities.Messages;
using OctarineCodex.Maps;
using OctarineCodex.Messaging;
using OctarineCodex.Services;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Makes camera follow player via message-based communication for better decoupling
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 900)]
public class CameraFollowBehavior : EntityBehavior, IMessageHandler<PlayerMovedMessage>
{
    private ICameraService _cameraService;
    private IMapService _mapService;
    private IWorldLayerService _worldLayerService;

    /// <summary>
    ///     Handle player movement messages to update camera position
    /// </summary>
    public void HandleMessage(PlayerMovedMessage message, string? senderId = null)
    {
        UpdateCameraPosition(message.NewPosition);
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        base.Initialize(entity, services);

        _cameraService = services.GetRequiredService<ICameraService>();
        _mapService = services.GetRequiredService<IMapService>();
        _worldLayerService = services.GetRequiredService<IWorldLayerService>();

        // Initial camera positioning
        UpdateCameraPosition(Entity.Position);
    }

    public override void Update(GameTime gameTime)
    {
        // Camera now updates via PlayerMovedMessage instead of polling position
        // Keep this method for potential future camera shake, smoothing, or other effects
    }

    private void UpdateCameraPosition(Vector2 playerPosition)
    {
        if (_cameraService == null || _mapService == null) return;

        Vector2 roomPosition;
        Vector2 roomSize;

        if (_mapService.CurrentLevels.Count > 1)
        {
            // Multi-level world: calculate current layer bounds
            var currentLayerLevels = _worldLayerService.GetCurrentLayerLevels();
            if (!currentLayerLevels.Any()) return;

            var minX = currentLayerLevels.Min(l => l.WorldX);
            var minY = currentLayerLevels.Min(l => l.WorldY);
            var maxX = currentLayerLevels.Max(l => l.WorldX + l.PxWid);
            var maxY = currentLayerLevels.Max(l => l.WorldY + l.PxHei);

            roomPosition = new Vector2(minX, minY);
            roomSize = new Vector2(maxX - minX, maxY - minY);
        }
        else
        {
            // Single level: use level bounds
            var bounds = _mapService.GetWorldBounds();
            roomPosition = new Vector2(bounds.X, bounds.Y);
            roomSize = new Vector2(bounds.Width, bounds.Height);
        }

        // Use existing Camera2D follow logic
        _cameraService.FollowTarget(playerPosition, OctarineConstants.PlayerSize, roomPosition, roomSize);
    }
}