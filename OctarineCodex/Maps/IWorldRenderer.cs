using System.Collections.Generic;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Maps;

public interface IWorldRenderer : ISimpleLevelRenderer
{
    Task LoadTilesetsForWorldAsync(IEnumerable<LDtkLevel> levels, ContentManager content);
    void RenderWorld(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera);
}

public class WorldRenderer : SimpleLevelRenderer, IWorldRenderer
{
    public async Task LoadTilesetsForWorldAsync(IEnumerable<LDtkLevel> levels, ContentManager content)
    {
        foreach (var level in levels) await LoadTilesetsAsync(level, content);
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