using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Domain.Characters;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Infrastructure.MonoGame;

namespace OctarineCodex.Application.Characters;

/// <summary>
///     Implementation of character customization for individual character instances
/// </summary>
public class CharacterCustomization : ICharacterCustomization // Add interface implementation
{
    private readonly LayeredAnimationController _animationController;
    private readonly IAsepriteAnimationService _asepriteService;
    private readonly IContentManagerService _contentManager;
    private readonly Dictionary<string, CharacterLayerDefinition> _layerDefinitions = new();
    private readonly Dictionary<string, Texture2D> _loadedTextures = new();
    private readonly ILoggingService _logger;

    private CharacterAppearance _currentAppearance = CharacterAppearance.Default;

    public CharacterCustomization(
        LayeredAnimationController animationController,
        IContentManagerService contentManager,
        IAsepriteAnimationService asepriteService,
        ILoggingService logger)
    {
        _animationController = animationController ?? throw new ArgumentNullException(nameof(animationController));
        _contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
        _asepriteService = asepriteService ?? throw new ArgumentNullException(nameof(asepriteService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void InitializeLayers(Dictionary<string, CharacterLayerDefinition> layerDefinitions)
    {
        _layerDefinitions.Clear();
        foreach (var (name, definition) in layerDefinitions)
        {
            _layerDefinitions[name] = definition;
        }

        // Load initial character with only the provided layers
        LoadCharacterAppearance(_currentAppearance);
    }

    public void ChangeLayerSelection(string layerName, int variantIndex)
    {
        if (!_layerDefinitions.TryGetValue(layerName, out CharacterLayerDefinition? layerDef))
        {
            throw new ArgumentException($"Unknown layer: {layerName}");
        }

        if (variantIndex >= layerDef.AvailableAssets.Length)
        {
            throw new ArgumentException($"Invalid variant index for {layerName}");
        }

        // Update appearance state
        var newSelections = new Dictionary<string, int>(_currentAppearance.LayerSelections)
        {
            [layerName] = variantIndex
        };
        _currentAppearance = new CharacterAppearance(newSelections);

        // Load and apply the new layer
        LoadLayer(layerName, variantIndex);
    }

    public CharacterAppearance GetCurrentAppearance()
    {
        return _currentAppearance;
    }

    public void Update(GameTime gameTime)
    {
        _animationController.Update(gameTime);
    }

    public void PlayAnimation(string animationName)
    {
        _logger.Debug($"CharacterCustomization: Playing animation {animationName}");
        _animationController.PlayAnimation(animationName);
    }

    public IEnumerable<LayerRenderData> GetLayerRenderData()
    {
        return _animationController.GetLayerRenderData();
    }

    private void LoadLayer(string layerName, int variantIndex)
    {
        if (!_layerDefinitions.TryGetValue(layerName, out CharacterLayerDefinition? layerDef))
        {
            _logger.Error($"Layer definition not found: {layerName}");
            return;
        }

        var assetName = layerDef.AvailableAssets[variantIndex];

        // Load texture using abstracted content service
        var textureKey = $"{layerName}_{variantIndex}";
        if (!_loadedTextures.ContainsKey(textureKey))
        {
            try
            {
                var filename = $"Character/{layerName}/{assetName}";
                _logger.Info($"Attempting to load {filename}");
                _loadedTextures[textureKey] = _contentManager.Load<Texture2D>(filename);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load texture for {layerName}: {ex.Message}");
                return;
            }
        }

        // Convert to animations based on format
        Dictionary<string, LDtkAnimationData> animations;

        try
        {
            if (layerDef.IsAsepriteFormat)
            {
                // Use new Aseprite JSON format
                animations = ConvertAsepriteToLDtkAnimations(layerDef.AsepriteJsonPath!);
            }
            else if (layerDef.Layout != null)
            {
                // Use legacy SpriteSheetLayout format
                animations = ConvertLayoutToAnimations(layerDef.Layout, _loadedTextures[textureKey]);
            }
            else
            {
                throw new InvalidOperationException($"Layer {layerName} has no valid animation format specified");
            }

            // Update animation controller
            _animationController.RemoveLayer(layerName);
            _animationController.AddLayer(layerName, animations, layerDef.Priority);

            _logger.Info($"Successfully loaded layer: {layerName}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to setup animations for layer {layerName}: {ex.Message}");
        }
    }

    private Dictionary<string, LDtkAnimationData> ConvertAsepriteToLDtkAnimations(string jsonPath)
    {
        Dictionary<string, AsepriteAnimation> asepriteAnimations = _asepriteService.LoadAnimations(jsonPath);
        var ldtkAnimations = new Dictionary<string, LDtkAnimationData>();

        var bridge = new AsepriteToLDtkBridge();
        foreach (var (name, animation) in asepriteAnimations)
        {
            ldtkAnimations[name] = bridge.ConvertToLDtkFormat(animation);
        }

        return ldtkAnimations;
    }

    private Dictionary<string, LDtkAnimationData> ConvertLayoutToAnimations(
        SpriteSheetLayout layout,
        Texture2D texture)
    {
        var animations = new Dictionary<string, LDtkAnimationData>();

        foreach (var (animName, animLayout) in layout.Animations)
        {
            var frameTileIds = new int[animLayout.FrameCount];
            for (var i = 0; i < animLayout.FrameCount; i++)
            {
                frameTileIds[i] = animLayout.StartTileId + i;
            }

            animations[animName] = new LDtkAnimationData(
                animName,
                frameTileIds,
                animLayout.FrameRate,
                animLayout.Loop
            );
        }

        return animations;
    }

    private void LoadCharacterAppearance(CharacterAppearance appearance)
    {
        // Only load layers that are actually defined
        foreach (var (layerName, variantIndex) in appearance.LayerSelections)
        {
            if (_layerDefinitions.ContainsKey(layerName))
            {
                LoadLayer(layerName, variantIndex);
            }
            else
            {
                _logger.Warn($"Skipping undefined layer: {layerName}");
            }
        }
    }
}
