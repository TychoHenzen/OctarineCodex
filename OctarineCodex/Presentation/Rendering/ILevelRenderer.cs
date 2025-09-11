using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Services;
using OctarineCodex.Presentation.Camera;

namespace OctarineCodex.Presentation.Rendering;

[Service<LevelRenderer>]
public interface ILevelRenderer
{
    void Initialize(GraphicsDevice graphicsDevice);
    void SetLDtkContext(LDtkFile file);
    void LoadTilesets(ContentManager content);

    void RenderLevelsBeforePlayer(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera,
        Vector2 playerPosition);

    void RenderLevelsAfterPlayer(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera,
        Vector2 playerPosition);

    void RenderForegroundLayers(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera,
        Vector2 playerPosition);
}
