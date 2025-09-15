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
///     Implementation of character customization for individual character instances.
/// </summary>
public class CharacterCustomization(
    ILayeredAnimationController animationController, // Changed from concrete to interface
    IContentManagerService contentManager,
    IAsepriteAnimationService asepriteService,
    ILoggingService logger)
    : ICharacterCustomization
{
    private readonly Dictionary<string, CharacterLayerDefinition> _layerDefinitions = new();
    private readonly Dictionary<string, Texture2D> _loadedTextures = new();

    private CharacterAppearance _currentAppearance = CharacterAppearance.Default;

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

        if (variantIndex >= layerDef.AvailableAssets.Count)
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
        animationController.Update(gameTime);
    }

    public void PlayAnimation(string animationName)
    {
        logger.Debug($"CharacterCustomization: Playing animation {animationName}");
        animationController.PlayAnimation(animationName);
    }

    public IEnumerable<LayerRenderData> GetLayerRenderData()
    {
        return animationController.GetLayerRenderData();
    }

    private void LoadLayer(string layerName, int variantIndex)
    {
        if (!_layerDefinitions.TryGetValue(layerName, out CharacterLayerDefinition? layerDef))
        {
            logger.Error($"Layer definition not found: {layerName}");
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
                logger.Info($"Attempting to load {filename}");
                _loadedTextures[textureKey] = contentManager.Load<Texture2D>(filename);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load texture for {layerName}: {ex.Message}");
                return;
            }
        }

        try
        {
            Dictionary<string, LDtkAnimationData> animations =
                ConvertAsepriteToLDtkAnimations(layerDef.JsonPath!);
            animationController.RemoveLayer(layerName);
            animationController.AddLayer(layerName, animations, layerDef.Priority);

            logger.Info($"Successfully loaded layer: {layerName}");
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to setup animations for layer {layerName}: {ex.Message}");
        }
    }

    private Dictionary<string, LDtkAnimationData> ConvertAsepriteToLDtkAnimations(string jsonPath)
    {
        Dictionary<string, AsepriteAnimation> asepriteAnimations = asepriteService.LoadAnimations(jsonPath);
        var ldtkAnimations = new Dictionary<string, LDtkAnimationData>();

        foreach (var (name, animation) in asepriteAnimations)
        {
            ldtkAnimations[name] = AsepriteToLDtkBridge.ConvertToLDtkFormat(animation);
        }

        return ldtkAnimations;
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
                logger.Warn($"Skipping undefined layer: {layerName}");
            }
        }
    }
}
