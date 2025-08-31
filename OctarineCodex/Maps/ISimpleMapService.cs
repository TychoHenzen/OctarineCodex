using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Maps;

/// <summary>
///     Simplified map service for loading and rendering a single LDtk level without complex room transitions.
/// </summary>
public interface ISimpleMapService
{
    /// <summary>
    ///     Gets the currently loaded level.
    /// </summary>
    LDtkLevel? CurrentLevel { get; }

    /// <summary>
    ///     Indicates if a level is currently loaded.
    /// </summary>
    bool IsLevelLoaded { get; }

    /// <summary>
    ///     Loads a single level from an LDtk file.
    /// </summary>
    /// <param name="filePath">Path to the LDtk file</param>
    /// <param name="levelIdentifier">Optional level identifier. If null, loads the first level.</param>
    /// <returns>The loaded level or null if loading failed</returns>
    Task<LDtkLevel?> LoadLevelAsync(string filePath, string? levelIdentifier = null);
}

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

    /// <summary>
    ///     Loads tilesets required for rendering the level.
    /// </summary>
    /// <param name="level">The level to load tilesets for</param>
    /// <param name="content">the content manager</param>
    Task LoadTilesetsAsync(LDtkLevel level, ContentManager content);

    /// <summary>
    ///     Renders the level centered on the screen.
    /// </summary>
    /// <param name="level">The level to render</param>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    /// <param name="screenCenter">Center point of the screen</param>
    void RenderLevel(LDtkLevel level, SpriteBatch spriteBatch, Vector2 screenCenter);
}