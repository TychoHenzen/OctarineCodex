using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Maps;

/// <summary>
///     Simplified renderer for displaying a single LDtk level centered on screen.
/// </summary>
public interface ISimpleLevelRenderer
{
    /// <summary>
    ///     Initializes the renderer with graphics device.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device</param>
    void Initialize(GraphicsDevice graphicsDevice);

    void SetLDtkContext(LDtkFile file);

    /// <summary>
    ///     Loads tilesets required for rendering the level.
    /// </summary>
    /// <param name="level">The level to load tilesets for</param>
    /// <param name="content">the content manager</param>
    Task LoadTilesetsAsync(ContentManager content);

    /// <summary>
    ///     Renders the level centered on the screen.
    /// </summary>
    /// <param name="level">The level to render</param>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    /// <param name="screenCenter">Center point of the screen</param>
    void RenderLevel(LDtkLevel level, SpriteBatch spriteBatch, Vector2 screenCenter);
}