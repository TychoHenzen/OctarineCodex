using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace OctarineCodex.Maps;

/// <summary>
/// Service for loading and managing LDTK maps.
/// Provides abstraction over LDTK file parsing and map data access.
/// </summary>
public interface ILdtkMapService
{
    /// <summary>
    /// Loads an LDTK project from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the LDTK project file.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded project or null if loading failed.</returns>
    Task<LdtkProject?> LoadProjectAsync(string filePath);

    /// <summary>
    /// Gets a level from the currently loaded project by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the level to retrieve.</param>
    /// <returns>The level with the specified identifier, or null if not found.</returns>
    LdtkLevel? GetLevel(string identifier);

    /// <summary>
    /// Gets all levels from the currently loaded project.
    /// </summary>
    /// <returns>An array of all levels in the project.</returns>
    LdtkLevel[] GetAllLevels();

    /// <summary>
    /// Gets the currently loaded project.
    /// </summary>
    /// <returns>The currently loaded project, or null if no project is loaded.</returns>
    LdtkProject? GetCurrentProject();

    /// <summary>
    /// Checks if a project is currently loaded.
    /// </summary>
    /// <returns>True if a project is loaded, false otherwise.</returns>
    bool IsProjectLoaded { get; }
}

/// <summary>
/// Service for rendering LDTK maps using MonoGame.
/// Provides methods to draw levels, layers, and entities.
/// </summary>
public interface ILdtkMapRenderer
{
    /// <summary>
    /// Initializes the renderer with the graphics device and content manager.
    /// Should be called during LoadContent phase.
    /// </summary>
    /// <param name="graphicsDevice">The MonoGame graphics device.</param>
    void Initialize(GraphicsDevice graphicsDevice);

    /// <summary>
    /// Loads tileset textures for rendering.
    /// </summary>
    /// <param name="project">The LDTK project containing tileset definitions.</param>
    /// <param name="contentPath">Base path for content files.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    Task LoadTilesetsAsync(LdtkProject project, string contentPath);

    /// <summary>
    /// Renders a level to the screen.
    /// </summary>
    /// <param name="level">The level to render.</param>
    /// <param name="spriteBatch">The sprite batch for drawing.</param>
    /// <param name="camera">Camera transformation matrix.</param>
    void RenderLevel(LdtkLevel level, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Matrix camera);

    /// <summary>
    /// Renders a specific layer from a level.
    /// </summary>
    /// <param name="layer">The layer to render.</param>
    /// <param name="spriteBatch">The sprite batch for drawing.</param>
    /// <param name="camera">Camera transformation matrix.</param>
    void RenderLayer(LdtkLayerInstance layer, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Matrix camera);

    /// <summary>
    /// Renders entities from an entity layer.
    /// </summary>
    /// <param name="entities">The entities to render.</param>
    /// <param name="spriteBatch">The sprite batch for drawing.</param>
    /// <param name="camera">Camera transformation matrix.</param>
    void RenderEntities(LdtkEntityInstance[] entities, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Matrix camera);
}

/// <summary>
/// Factory for creating map-related services.
/// Follows the factory pattern for managing complex object creation.
/// </summary>
public interface ILdtkMapFactory
{
    /// <summary>
    /// Creates a new instance of the LDTK map service.
    /// </summary>
    /// <returns>A new LDTK map service instance.</returns>
    ILdtkMapService CreateMapService();

    /// <summary>
    /// Creates a new instance of the LDTK map renderer.
    /// </summary>
    /// <returns>A new LDTK map renderer instance.</returns>
    ILdtkMapRenderer CreateMapRenderer();
}

/// <summary>
/// Result wrapper for LDTK operations that can fail.
/// Provides error handling without exceptions for performance-critical scenarios.
/// </summary>
/// <typeparam name="T">The type of the successful result.</typeparam>
public record LdtkResult<T>
{
    public T? Value { get; init; }
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    public static LdtkResult<T> Success(T value) => new() { Value = value, IsSuccess = true };
    public static LdtkResult<T> Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}