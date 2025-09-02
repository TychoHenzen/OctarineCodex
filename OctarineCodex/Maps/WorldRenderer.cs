using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

public class WorldRenderer : SimpleLevelRenderer, IWorldRenderer
{
    public WorldRenderer(ILoggingService logger) : base(logger)
    {
    }

    public void RenderWorld(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera)
    {
        foreach (var level in levels)
        {
            // Calculate visible bounds from camera position and viewport
            var cameraBounds = new Rectangle(
                (int)camera.Position.X,
                (int)camera.Position.Y,
                (int)camera.ViewportSize.X,
                (int)camera.ViewportSize.Y
            );

            var levelBounds = new Rectangle(level.WorldX, level.WorldY, level.PxWid, level.PxHei);

            if (cameraBounds.Intersects(levelBounds))
            {
                var levelOffset = new Vector2(level.WorldX, level.WorldY);
                RenderLevel(level, spriteBatch, levelOffset);
            }
        }
    }
}