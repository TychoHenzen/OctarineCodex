using System;
using System.Linq;
using Microsoft.Xna.Framework;
using OctarineCodex.Maps;
using OctarineCodex.Services;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Makes camera follow player using existing Camera2D logic
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 900)]
public class CameraFollowBehavior : EntityBehavior
{
    private ICameraService _cameraService;
    private IMapService _mapService;
    private IWorldLayerService _worldLayerService;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        base.Initialize(entity, services);

        _cameraService = (ICameraService)services.GetService(typeof(ICameraService));
        _mapService = (IMapService)services.GetService(typeof(IMapService));
        _worldLayerService = (IWorldLayerService)services.GetService(typeof(IWorldLayerService));
    }

    public override void Update(GameTime gameTime)
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
        _cameraService.FollowTarget(Entity.Position, OctarineConstants.PlayerSize, roomPosition, roomSize);
    }
}