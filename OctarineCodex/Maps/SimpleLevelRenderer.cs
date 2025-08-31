using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Maps;

/// <summary>
///     Simple implementation of ISimpleLevelRenderer for displaying a single LDtk level centered on screen.
/// </summary>
public sealed class SimpleLevelRenderer : ISimpleLevelRenderer, IDisposable
{
    private readonly Dictionary<string, Texture2D> _loadedTextures = new();
    private readonly Dictionary<int, TilesetDefinition> _tilesetDefinitions = new();
    private GraphicsDevice? _graphicsDevice;

    public void Dispose()
    {
        foreach (var texture in _loadedTextures.Values) texture?.Dispose();
        _loadedTextures.Clear();
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    }

    public async Task LoadTilesetsAsync(LDtkLevel level, ContentManager content)
    {
        if (_graphicsDevice == null)
            throw new InvalidOperationException("GraphicsDevice must be initialized before loading tilesets");

        // Clear existing textures
        _loadedTextures.Clear();
        _tilesetDefinitions.Clear();

        // Load tilesets from the level's layer instances
        if (level.LayerInstances != null)
            foreach (var layer in level.LayerInstances.Select(layer => layer._TilesetDefUid))
                if (layer.HasValue)
                {
                    var tilesetUid = layer.Value;
                    var tilesetKey = $"tileset_{tilesetUid}";

                    if (!_loadedTextures.ContainsKey(tilesetKey))
                    {
                        // Find tileset definition in the level's world
                        var tilesetDef = GetTilesetDefinition(level, tilesetUid);
                        if (tilesetDef != null)
                        {
                            _tilesetDefinitions[tilesetUid] = tilesetDef;

                            // Try to load the texture from content path
                            var texture = await LoadTilesetTextureAsync(tilesetDef, content);
                            if (texture != null) _loadedTextures[tilesetKey] = texture;
                        }
                    }
                }
    }

    public void RenderLevel(LDtkLevel level, SpriteBatch spriteBatch, Vector2 screenCenter)
    {
        if (_graphicsDevice == null)
            throw new InvalidOperationException("GraphicsDevice must be initialized before rendering");

        if (level == null)
            return;

        // Draw level background
        var levelBounds = new Rectangle(
            (int)screenCenter.X,
            (int)screenCenter.Y,
            level.PxWid,
            level.PxHei
        );
        var pixelTexture = GetOrCreatePixelTexture();
        spriteBatch.Draw(pixelTexture, levelBounds, Color.DarkGray * 0.3f);

        // Render each layer in the level
        foreach (var layer in level.LayerInstances)
            RenderLayer(layer, spriteBatch, screenCenter);

        // Draw level border for debugging
        var borderThickness = 1;
        spriteBatch.Draw(pixelTexture, new Rectangle(levelBounds.X, levelBounds.Y, levelBounds.Width, borderThickness),
            Color.White * 0.5f);
        spriteBatch.Draw(pixelTexture,
            new Rectangle(levelBounds.X, levelBounds.Bottom - borderThickness, levelBounds.Width, borderThickness),
            Color.White * 0.5f);
        spriteBatch.Draw(pixelTexture, new Rectangle(levelBounds.X, levelBounds.Y, borderThickness, levelBounds.Height),
            Color.White * 0.5f);
        spriteBatch.Draw(pixelTexture,
            new Rectangle(levelBounds.Right - borderThickness, levelBounds.Y, borderThickness, levelBounds.Height),
            Color.White * 0.5f);
    }

    private void RenderLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 levelOffset)
    {
        // Apply layer offset
        var layerOffset = levelOffset + new Vector2(layer.PxOffsetX, layer.PxOffsetY);

        switch (layer._Type)
        {
            case LayerType.IntGrid:
                // IntGrid layers with auto-tiles should render sprites, not solid colors
                if (layer.AutoLayerTiles.Any())
                    RenderTileLayer(layer, spriteBatch, layerOffset);
                else
                    RenderIntGridLayer(layer, spriteBatch, layerOffset);
                break;
            case LayerType.Tiles:
            case LayerType.AutoLayer:
                RenderTileLayer(layer, spriteBatch, layerOffset);
                break;
            case LayerType.Entities:
                RenderEntityLayer(layer, spriteBatch, layerOffset);
                break;
        }
    }

    private void RenderIntGridLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 offset)
    {
        var pixelTexture = GetOrCreatePixelTexture();
        var colors = new[] { Color.Transparent, Color.Red * 0.6f, Color.Green * 0.6f, Color.Blue * 0.6f };

        for (var i = 0; i < layer.IntGridCsv.Length; i++)
        {
            var value = layer.IntGridCsv[i];
            if (value == 0) continue; // Skip empty cells

            var x = (int)offset.X + i % layer._CWid * layer._GridSize;
            var y = (int)offset.Y + i / layer._CWid * layer._GridSize;
            var color = colors[Math.Min(value, colors.Length - 1)];

            var destRect = new Rectangle(x, y, layer._GridSize, layer._GridSize);
            spriteBatch.Draw(pixelTexture, destRect, color);
        }
    }

    private void RenderTileLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 offset)
    {
        if (!layer._TilesetDefUid.HasValue) return;

        var tilesetKey = $"tileset_{layer._TilesetDefUid.Value}";
        if (!_loadedTextures.TryGetValue(tilesetKey, out var tileset)) return;

        // Render auto layer tiles
        foreach (var tile in layer.AutoLayerTiles)
        {
            var sourceRect = new Rectangle(tile.Src.X, tile.Src.Y, layer._GridSize, layer._GridSize);
            var destRect = new Rectangle(
                (int)offset.X + tile.Px.X,
                (int)offset.Y + tile.Px.Y,
                layer._GridSize,
                layer._GridSize
            );

            var effects = SpriteEffects.None;
            if ((tile.F & 1) != 0) effects |= SpriteEffects.FlipHorizontally;
            if ((tile.F & 2) != 0) effects |= SpriteEffects.FlipVertically;

            spriteBatch.Draw(tileset, destRect, sourceRect, Color.White, 0f, Vector2.Zero, effects, 0f);
        }

        // Render grid tiles
        foreach (var tile in layer.GridTiles)
        {
            var sourceRect = new Rectangle(tile.Src.X, tile.Src.Y, layer._GridSize, layer._GridSize);
            var destRect = new Rectangle(
                (int)offset.X + tile.Px.X,
                (int)offset.Y + tile.Px.Y,
                layer._GridSize,
                layer._GridSize
            );

            var effects = SpriteEffects.None;
            if ((tile.F & 1) != 0) effects |= SpriteEffects.FlipHorizontally;
            if ((tile.F & 2) != 0) effects |= SpriteEffects.FlipVertically;

            spriteBatch.Draw(tileset, destRect, sourceRect, Color.White, 0f, Vector2.Zero, effects, 0f);
        }
    }

    private void RenderEntityLayer(LayerInstance layer, SpriteBatch spriteBatch, Vector2 offset)
    {
        var pixelTexture = GetOrCreatePixelTexture();

        foreach (var entity in layer.EntityInstances)
        {
            var bounds = new Rectangle(
                (int)offset.X + entity.Px.X,
                (int)offset.Y + entity.Px.Y,
                entity.Width,
                entity.Height
            );

            // Draw entity as colored rectangle
            spriteBatch.Draw(pixelTexture, bounds, Color.Cyan * 0.5f);
        }
    }

    private TilesetDefinition? GetTilesetDefinition(LDtkLevel level, int tilesetUid)
    {
        // Access tileset definitions from the level's world
        return level.LayerInstances
            .Where(l => l._TilesetDefUid == tilesetUid)
            .Select(l => new TilesetDefinition
            {
                Uid = tilesetUid,
                RelPath = l._TilesetRelPath ?? "",
                PxWid = 96, // Default values - should come from actual definition
                PxHei = 256,
                TileGridSize = l._GridSize
            })
            .FirstOrDefault();
    }

    private async Task<Texture2D?> LoadTilesetTextureAsync(TilesetDefinition tilesetDef, ContentManager content)
    {
        try
        {
            var fileName = Path.GetFileName(tilesetDef.RelPath);
            var textureName = Path.GetFileNameWithoutExtension(fileName);

            // Try content pipeline first
            try
            {
                // Need ContentManager reference - modify constructor to accept it
                return content.Load<Texture2D>($"atlas/{textureName}");
            }
            catch
            {
                // Create fallback colored texture
                var fallback = new Texture2D(_graphicsDevice!, tilesetDef.TileGridSize, tilesetDef.TileGridSize);
                var colorData = new Color[tilesetDef.TileGridSize * tilesetDef.TileGridSize];
                Array.Fill(colorData, Color.Magenta);
                fallback.SetData(colorData);
                return fallback;
            }
        }
        catch
        {
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
}