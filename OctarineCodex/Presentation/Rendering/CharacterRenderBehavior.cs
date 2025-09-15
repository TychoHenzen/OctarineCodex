using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Entities;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Domain.Entities;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Infrastructure.MonoGame;

namespace OctarineCodex.Presentation.Rendering;

/// <summary>
///     Unified character rendering system that works with the CharacterCustomizationSystem
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 50)]
public class CharacterRenderBehavior : EntityBehavior
{
    private readonly IContentManagerService _contentManager;
    private readonly Dictionary<string, Texture2D> _layerTextures = new();
    private readonly ILoggingService _logger;
    private bool _isInitialized;

    public CharacterRenderBehavior(ILoggingService logger, IContentManagerService contentManager)
    {
        _contentManager = contentManager;
        _logger = logger;
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity)
    {
        base.Initialize(entity);
        LoadCharacterTextures();
        _isInitialized = true;
    }

    public override void Draw(SpriteBatch? spriteBatch)
    {
        if (spriteBatch == null || !_isInitialized)
        {
            return;
        }

        // Get the character system from PlayerCharacterAnimationBehavior
        var animationBehavior = Entity.GetBehavior<PlayerCharacterAnimationBehavior>();
        if (animationBehavior?.CharacterSystem == null)
        {
            return;
        }

        // Get layer render data from the character system
        IEnumerable<LayerRenderData> layerRenderData = animationBehavior.CharacterSystem.GetLayerRenderData();

        var layerCount = 0;
        foreach (LayerRenderData layer in layerRenderData.OrderBy(l => l.Priority))
        {
            layerCount++;
            if (_layerTextures.TryGetValue(layer.LayerName, out Texture2D? texture))
            {
                DrawCharacterLayer(spriteBatch, texture, layer);
            }
        }

        // Debug: Log if no layers are being rendered
        if (layerCount == 0)
        {
            _logger.Warn("No character layers being rendered!");
        }
    }

    private void DrawCharacterLayer(SpriteBatch spriteBatch, Texture2D texture, LayerRenderData layer)
    {
        // Calculate source rectangle from tile ID
        Rectangle sourceRect = CalculateSourceRect(layer.TileId);

        // Debug output
        _logger.Info($"Rendering layer {layer.LayerName}, TileID: {layer.TileId}, SourceRect: {sourceRect}");

        // Character size and position
        var destRect = new Rectangle(
            (int)Entity.Position.X,
            (int)Entity.Position.Y,
            16, // Character width
            32 // Character height
        );

        spriteBatch.Draw(
            texture,
            destRect,
            sourceRect,
            Color.White * layer.Alpha);
    }

    private Rectangle CalculateSourceRect(int tileId)
    {
        // Calculate source rectangle based on tile ID
        // Based on the animation.json structure: 16x32 tiles, 56 tiles per row (896px / 16px)
        const int tileWidth = 16;
        const int tileHeight = 32;
        const int tilesPerRow = 56;

        var tileX = tileId % tilesPerRow;
        var tileY = tileId / tilesPerRow;

        return new Rectangle(
            tileX * tileWidth,
            tileY * tileHeight,
            tileWidth,
            tileHeight);
    }

    private void LoadCharacterTextures()
    {
        // Load the same textures that CharacterCustomization expects
        var layerNames = new[] { "Bodies", "Eyes", "Hairstyles", "Outfits", "Accessories" };

        foreach (var layerName in layerNames)
        {
            try
            {
                // Load the default texture for each layer
                var texturePath = layerName switch
                {
                    "Bodies" => "Character/Bodies/Body_01",
                    "Eyes" => "Character/Eyes/Eyes_01",
                    "Hairstyles" => "Character/Hairstyles/Hairstyle_01_01",
                    "Outfits" => "Character/Outfits/Outfit_01_01",
                    "Accessories" => "Character/Accessories/Accessory_01_Ladybug_01",
                    _ => $"Character/{layerName}/{layerName}_01"
                };

                _layerTextures[layerName] = _contentManager.Load<Texture2D>(texturePath);
            }
            catch
            {
                // If texture fails to load, skip this layer
                // This allows the system to work even if some character parts are missing
            }
        }
    }
}
