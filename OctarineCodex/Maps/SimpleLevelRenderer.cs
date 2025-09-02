using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

public class SimpleLevelRenderer : ISimpleLevelRenderer, IDisposable
{
    private readonly Dictionary<string, Texture2D> _loadedTextures = new();
    protected readonly ILoggingService _logger;
    private GraphicsDevice? _graphicsDevice;
    private LDtkFile? _ldtkFile; // Store file reference for definitions

    public SimpleLevelRenderer(ILoggingService logger)
    {
        _logger = logger;
    }

    public void Dispose()
    {
        foreach (var texture in _loadedTextures.Values.Where(t => t != null))
            texture?.Dispose();
        _loadedTextures.Clear();
        GC.SuppressFinalize(this);
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    }

    public async Task LoadTilesetsAsync(ContentManager content)
    {
        if (_graphicsDevice == null)
            throw new InvalidOperationException("GraphicsDevice must be initialized before loading tilesets");

        _logger.Debug($"_ldtkFile is null: {_ldtkFile == null}");
        _logger.Debug($"_ldtkFile.Defs is null: {_ldtkFile?.Defs == null}");
        _logger.Debug($"_ldtkFile.Defs.Tilesets is null: {_ldtkFile?.Defs?.Tilesets == null}");
        _logger.Debug($"Tileset count: {_ldtkFile?.Defs?.Tilesets?.Length ?? 0}");

        if (_ldtkFile?.Defs?.Tilesets == null)
        {
            _logger.Debug("Early return: no tilesets found in _ldtkFile");
            return;
        }

        // Clear and reload textures
        foreach (var texture in _loadedTextures.Values.Where(t => t != null))
            texture?.Dispose();
        _loadedTextures.Clear();

        _logger.Debug($"Loading {_ldtkFile.Defs.Tilesets.Length} tilesets...");

        // Load tilesets from file definitions
        foreach (var tilesetDef in _ldtkFile.Defs.Tilesets)
        {
            var tilesetKey = $"tileset_{tilesetDef.Uid}";
            _logger.Debug($"Processing tileset: {tilesetKey}, RelPath: {tilesetDef.RelPath}");

            if (!_loadedTextures.ContainsKey(tilesetKey))
            {
                var texture = await LoadTilesetTextureAsync(tilesetDef, content);
                if (texture != null)
                {
                    _loadedTextures[tilesetKey] = texture;
                    _logger.Debug($"Successfully loaded tileset: {tilesetKey} ({texture.Width}x{texture.Height})");
                }
                else
                {
                    _logger.Debug($"Failed to load tileset: {tilesetKey}");
                }
            }
        }

        _logger.Debug($"Total loaded textures: {_loadedTextures.Count}");
        foreach (var kvp in _loadedTextures) _logger.Debug($"  - {kvp.Key}: {kvp.Value?.Width}x{kvp.Value?.Height}");
    }

    public void RenderLevel(LDtkLevel level, SpriteBatch spriteBatch, Vector2 screenCenter)
    {
        if (_graphicsDevice == null || level == null) return;

        // Draw level background
        var levelBounds = new Rectangle((int)screenCenter.X, (int)screenCenter.Y, level.PxWid, level.PxHei);
        var pixelTexture = GetOrCreatePixelTexture();
        spriteBatch.Draw(pixelTexture, levelBounds, Color.DarkGray * 0.3f);


        if (level.LayerInstances != null)
            foreach (var layer in level.LayerInstances.Reverse())
            {
                // Skip invisible layers
                if (!layer.Visible) continue;

                RenderLayer(layer, spriteBatch, screenCenter);
            }
    }

    public void SetLDtkContext(LDtkFile file)
    {
        _ldtkFile = file;
        _logger.Debug($"SetLDtkContext called. File has {file?.Defs?.Tilesets?.Length ?? 0} tilesets");
    }

    private void RenderLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 levelOffset)
    {
        // Calculate layer opacity
        var layerOpacity = layer._Opacity;
        if (layerOpacity <= 0) return; // Skip fully transparent layers

        // Apply layer offset
        var layerOffset = levelOffset + new Vector2(layer._PxTotalOffsetX, layer._PxTotalOffsetY);

        switch (layer._Type)
        {
            case LayerType.IntGrid:
                if (layer.AutoLayerTiles.Any())
                    RenderTileLayer(layer, spriteBatch, layerOffset, layerOpacity);
                else
                    RenderIntGridLayer(layer, spriteBatch, layerOffset, layerOpacity);
                break;

            case LayerType.Tiles:
            case LayerType.AutoLayer:
                RenderTileLayer(layer, spriteBatch, layerOffset, layerOpacity);
                break;

            case LayerType.Entities:
                break;
        }
    }

    private void RenderTileLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 offset, float opacity)
    {
        if (!layer._TilesetDefUid.HasValue)
        {
            _logger.Debug($"Layer {layer._Identifier} has no tileset UID");
            return;
        }

        var tilesetKey = $"tileset_{layer._TilesetDefUid.Value}";

        if (!_loadedTextures.TryGetValue(tilesetKey, out var tileset))
        {
            _logger.Debug($"Tileset not found: {tilesetKey}");
            _logger.Debug($"Available tilesets: {string.Join(", ", _loadedTextures.Keys)}");
            return;
        }

        var layerColor = Color.White * opacity;

        // Render auto layer tiles (these include the auto-rule generated tiles)
        foreach (var tile in layer.AutoLayerTiles)
            RenderTile(tile, tileset, spriteBatch, offset, layer._GridSize, layerColor);

        // Render grid tiles (manually placed tiles)
        foreach (var tile in layer.GridTiles)
            RenderTile(tile, tileset, spriteBatch, offset, layer._GridSize, layerColor);
    }

    private void RenderTile(TileInstance tile, Texture2D tileset, SpriteBatch spriteBatch, Vector2 offset, int gridSize,
        Color layerColor)
    {
        var sourceRect = new Rectangle(tile.Src.X, tile.Src.Y, gridSize, gridSize);
        var destRect = new Rectangle(
            (int)offset.X + tile.Px.X,
            (int)offset.Y + tile.Px.Y,
            gridSize,
            gridSize
        );

        // Handle tile flipping
        var effects = SpriteEffects.None;
        if ((tile.F & 1) != 0) effects |= SpriteEffects.FlipHorizontally;
        if ((tile.F & 2) != 0) effects |= SpriteEffects.FlipVertically;

        // Apply tile alpha and layer opacity
        var tileColor = layerColor * tile.A;

        spriteBatch.Draw(tileset, destRect, sourceRect, tileColor, 0f, Vector2.Zero, effects, 0f);
    }

    private void RenderIntGridLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 offset, float opacity)
    {
        var pixelTexture = GetOrCreatePixelTexture();
        var baseColors = new[] { Color.Transparent, Color.Red, Color.Green, Color.Blue, Color.Yellow };

        var cellsRendered = 0;
        for (var i = 0; i < layer.IntGridCsv.Length; i++)
        {
            var value = layer.IntGridCsv[i];
            if (value == 0) continue;

            var x = (int)offset.X + i % layer._CWid * layer._GridSize;
            var y = (int)offset.Y + i / layer._CWid * layer._GridSize;
            var color = baseColors[Math.Min(value, baseColors.Length - 1)] * (0.6f * opacity);

            var destRect = new Rectangle(x, y, layer._GridSize, layer._GridSize);
            spriteBatch.Draw(pixelTexture, destRect, color);
            cellsRendered++;
        }

        Console.WriteLine($"Rendered {cellsRendered} IntGrid cells for layer {layer._Identifier}");
    }

    private async Task<Texture2D?> LoadTilesetTextureAsync(TilesetDefinition tilesetDef, ContentManager content)
    {
        try
        {
            if (string.IsNullOrEmpty(tilesetDef.RelPath))
            {
                Console.WriteLine($"Empty RelPath for tileset UID {tilesetDef.Uid}");
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(tilesetDef.RelPath);
            _logger.Debug($"Attempting to load texture for file: {fileName}");

            // Try different content paths
            var possiblePaths = new[]
            {
                fileName, // "TopDown_by_deepnight"
                $"atlas/{fileName}", // Keep existing fallbacks
                $"tilesets/{fileName}",
                $"sprites/{fileName}"
            };

            foreach (var path in possiblePaths)
                try
                {
                    var texture = content.Load<Texture2D>(path);
                    _logger.Debug($"Successfully loaded texture from content pipeline: {path}");
                    return texture;
                }
                catch (Exception ex)
                {
                    _logger.Debug($"Failed to load texture path '{path}': {ex.Message}");
                }

            _logger.Debug(
                $"All content pipeline paths failed, creating debug texture ({tilesetDef.PxWid}x{tilesetDef.PxHei})");

            // If all content pipeline attempts fail, create a debug texture with correct dimensions
            var debugTexture = new Texture2D(_graphicsDevice!, tilesetDef.PxWid, tilesetDef.PxHei);
            var colorData = new Color[tilesetDef.PxWid * tilesetDef.PxHei];

            // Create a pattern to help identify missing tilesets
            for (var i = 0; i < colorData.Length; i++)
            {
                var x = i % tilesetDef.PxWid;
                var y = i / tilesetDef.PxWid;
                var tileX = x / tilesetDef.TileGridSize;
                var tileY = y / tilesetDef.TileGridSize;
                colorData[i] = (tileX + tileY) % 2 == 0 ? Color.Magenta : Color.Black;
            }

            debugTexture.SetData(colorData);
            _logger.Debug($"Created debug checkerboard texture ({debugTexture.Width}x{debugTexture.Height})");
            return debugTexture;
        }
        catch (Exception ex)
        {
            _logger.Debug($"Failed to create any texture for tileset {tilesetDef.RelPath}: {ex.Message}");
            _logger.Debug($"Failed to load tileset {tilesetDef.RelPath}: {ex.Message}");
            return null;
        }
    }

    private Texture2D GetOrCreatePixelTexture()
    {
        if (!_loadedTextures.ContainsKey("__pixel__"))
        {
            var texture = new Texture2D(_graphicsDevice!, 1, 1);
            texture.SetData(new[] { Color.White });
            _loadedTextures["__pixel__"] = texture;
        }

        return _loadedTextures["__pixel__"];
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}