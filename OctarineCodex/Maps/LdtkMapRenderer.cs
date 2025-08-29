using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OctarineCodex.Maps;

/// <summary>
/// Implementation of LDTK map renderer for MonoGame.
/// Handles rendering of levels, layers, tiles, and entities.
/// </summary>
public sealed class LdtkMapRenderer : ILdtkMapRenderer
{
    private GraphicsDevice? _graphicsDevice;
    private readonly Dictionary<int, Texture2D> _tilesetTextures = new();
    private Texture2D? _pixelTexture;

    /// <summary>
    /// Initializes the renderer with the graphics device.
    /// Should be called during LoadContent phase.
    /// </summary>
    /// <param name="graphicsDevice">The MonoGame graphics device.</param>
    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        
        // Create a 1x1 white pixel texture for drawing colored rectangles
        _pixelTexture = new Texture2D(_graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        _pixelTexture.SetData(new[] { Color.White });
    }

    /// <summary>
    /// Loads tileset textures for rendering.
    /// </summary>
    /// <param name="project">The LDTK project containing tileset definitions.</param>
    /// <param name="contentPath">Base path for content files.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public async Task LoadTilesetsAsync(LdtkProject project, string contentPath)
    {
        if (_graphicsDevice is null)
        {
            throw new InvalidOperationException("Renderer must be initialized before loading tilesets");
        }

        foreach (var tileset in project.Definitions.Tilesets)
        {
            try
            {
                // For now, we'll create placeholder textures since we don't have actual tileset images
                // In a real implementation, you would load the actual tileset images from tileset.RelPath
                var texture = new Texture2D(_graphicsDevice, tileset.PixelWidth, tileset.PixelHeight);
                var colorData = new Color[tileset.PixelWidth * tileset.PixelHeight];
                
                // Fill with a checkerboard pattern for visualization
                for (int i = 0; i < colorData.Length; i++)
                {
                    var x = i % tileset.PixelWidth;
                    var y = i / tileset.PixelWidth;
                    var tileX = x / tileset.TileGridSize;
                    var tileY = y / tileset.TileGridSize;
                    colorData[i] = ((tileX + tileY) % 2 == 0) ? Color.LightBlue : Color.DarkBlue;
                }
                
                texture.SetData(colorData);
                _tilesetTextures[tileset.Uid] = texture;
            }
            catch (Exception ex)
            {
                // Log error but continue with other tilesets
                Console.WriteLine($"Failed to load tileset {tileset.Identifier}: {ex.Message}");
            }
        }

        await Task.CompletedTask; // Placeholder for actual async loading
    }

    /// <summary>
    /// Renders a level to the screen.
    /// </summary>
    /// <param name="level">The level to render.</param>
    /// <param name="spriteBatch">The sprite batch for drawing.</param>
    /// <param name="camera">Camera transformation matrix.</param>
    public void RenderLevel(LdtkLevel level, SpriteBatch spriteBatch, Matrix camera)
    {
        if (_graphicsDevice is null || _pixelTexture is null)
        {
            throw new InvalidOperationException("Renderer must be initialized before rendering");
        }

        // Render layers in order (background to foreground)
        foreach (var layer in level.LayerInstances)
        {
            RenderLayer(layer, spriteBatch, camera);
        }
    }

    /// <summary>
    /// Renders a specific layer from a level.
    /// </summary>
    /// <param name="layer">The layer to render.</param>
    /// <param name="spriteBatch">The sprite batch for drawing.</param>
    /// <param name="camera">Camera transformation matrix.</param>
    public void RenderLayer(LdtkLayerInstance layer, SpriteBatch spriteBatch, Matrix camera)
    {
        if (_graphicsDevice is null || _pixelTexture is null)
        {
            throw new InvalidOperationException("Renderer must be initialized before rendering");
        }

        switch (layer.Type.ToLowerInvariant())
        {
            case "tiles":
                RenderTileLayer(layer, spriteBatch, camera);
                break;
            case "intgrid":
                RenderIntGridLayer(layer, spriteBatch, camera);
                break;
            case "entities":
                RenderEntities(layer.EntityInstances, spriteBatch, camera);
                break;
            case "autolayer":
                RenderTileLayer(layer, spriteBatch, camera); // Treat auto layers as tile layers
                break;
        }
    }

    /// <summary>
    /// Renders entities from an entity layer.
    /// </summary>
    /// <param name="entities">The entities to render.</param>
    /// <param name="spriteBatch">The sprite batch for drawing.</param>
    /// <param name="camera">Camera transformation matrix.</param>
    public void RenderEntities(LdtkEntityInstance[] entities, SpriteBatch spriteBatch, Matrix camera)
    {
        if (_pixelTexture is null)
        {
            throw new InvalidOperationException("Renderer must be initialized before rendering");
        }

        foreach (var entity in entities)
        {
            var position = new Vector2(entity.Px[0], entity.Px[1]);
            var size = new Vector2(entity.Width, entity.Height);
            var bounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            
            // Render entity as a colored rectangle for visualization
            var color = ParseHexColor("#FF0000"); // Default red color
            spriteBatch.Draw(_pixelTexture, bounds, color * 0.7f); // Semi-transparent
            
            // Draw entity border
            DrawRectangleOutline(spriteBatch, bounds, color, 1);
        }
    }

    private void RenderTileLayer(LdtkLayerInstance layer, SpriteBatch spriteBatch, Matrix camera)
    {
        if (layer.TilesetDefUid is null || !_tilesetTextures.TryGetValue(layer.TilesetDefUid.Value, out var tileset))
        {
            return; // No tileset available
        }

        foreach (var tile in layer.GridTiles)
        {
            var destPosition = new Vector2(tile.Px[0], tile.Px[1]);
            var sourceRect = new Rectangle(tile.Src[0], tile.Src[1], layer.GridSize, layer.GridSize);
            var destRect = new Rectangle((int)destPosition.X, (int)destPosition.Y, layer.GridSize, layer.GridSize);
            
            var effects = SpriteEffects.None;
            if ((tile.FlipBits & 1) != 0) effects |= SpriteEffects.FlipHorizontally;
            if ((tile.FlipBits & 2) != 0) effects |= SpriteEffects.FlipVertically;
            
            spriteBatch.Draw(tileset, destRect, sourceRect, Color.White, 0f, Vector2.Zero, effects, 0f);
        }
    }

    private void RenderIntGridLayer(LdtkLayerInstance layer, SpriteBatch spriteBatch, Matrix camera)
    {
        if (_pixelTexture is null)
        {
            return;
        }

        // Render IntGrid as colored tiles
        var colors = new[] { Color.Transparent, Color.Red, Color.Green, Color.Blue, Color.Yellow };
        
        for (int i = 0; i < layer.IntGridCsv.Length; i++)
        {
            var value = layer.IntGridCsv[i];
            if (value == 0) continue; // Skip empty cells
            
            var x = (i % layer.CellWidth) * layer.GridSize;
            var y = (i / layer.CellWidth) * layer.GridSize;
            var color = colors[Math.Min(value, colors.Length - 1)];
            
            var destRect = new Rectangle(x, y, layer.GridSize, layer.GridSize);
            spriteBatch.Draw(_pixelTexture, destRect, color * 0.8f);
        }
    }

    private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
    {
        if (_pixelTexture is null) return;

        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private static Color ParseHexColor(string hex)
    {
        if (hex.StartsWith('#'))
            hex = hex[1..];
            
        if (hex.Length == 6)
        {
            var r = Convert.ToByte(hex[0..2], 16);
            var g = Convert.ToByte(hex[2..4], 16);
            var b = Convert.ToByte(hex[4..6], 16);
            return new Color(r, g, b);
        }
        
        return Color.White;
    }
}