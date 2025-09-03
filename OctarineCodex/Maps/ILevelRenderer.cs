using System.Collections.Generic;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Player;

namespace OctarineCodex.Maps;

public interface ILevelRenderer
{
    void Initialize(GraphicsDevice graphicsDevice);
    void SetLDtkContext(LDtkFile file);
    Task LoadTilesetsAsync(ContentManager content);

    // Split rendering methods for proper player depth
    void RenderLevelsBeforePlayer(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera);
    void RenderLevelsAfterPlayer(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera);
}