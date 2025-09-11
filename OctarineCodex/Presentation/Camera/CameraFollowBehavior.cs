// OctarineCodex/Entities/Behaviors/CameraFollowBehavior.cs

using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Maps;
using OctarineCodex.Application.Messages;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Domain.Entities;

namespace OctarineCodex.Presentation.Camera;

/// <summary>
///     Makes camera follow player via message-based communication for better decoupling.
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 900)]
public class CameraFollowBehavior(
    ICameraService cameraService,
    IMapService mapService,
    IWorldLayerService worldLayerService)
    : EntityBehavior, IMessageHandler<PlayerMovedMessage>
{
    /// <summary>
    ///     Handle player movement messages to update camera position.
    /// </summary>
    public void HandleMessage(PlayerMovedMessage message, string? senderId = null)
    {
        UpdateCameraPosition(message.NewPosition);
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity)
    {
        base.Initialize(entity);

        // Initial camera positioning
        UpdateCameraPosition(Entity.Position);
    }

    private void UpdateCameraPosition(Vector2 playerPosition)
    {
        Vector2 roomPosition;
        Vector2 roomSize;

        if (mapService.CurrentLevels.Count > 1)
        {
            // Multi-level world: calculate current layer bounds
            IReadOnlyList<LDtkLevel> currentLayerLevels = worldLayerService.GetCurrentLayerLevels();
            if (!currentLayerLevels.Any())
            {
                return;
            }

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
            Rectangle bounds = mapService.GetWorldBounds();
            roomPosition = new Vector2(bounds.X, bounds.Y);
            roomSize = new Vector2(bounds.Width, bounds.Height);
        }

        // Use existing Camera2D follow logic
        cameraService.FollowTarget(playerPosition, OctarineConstants.PlayerSize, roomPosition, roomSize);
    }
}
