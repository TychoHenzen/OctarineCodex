// OctarineCodex/Application/Characters/CharacterCustomizationSystem.cs

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Domain.Characters;
using OctarineCodex.Infrastructure.MonoGame;

namespace OctarineCodex.Application.Characters;

public class CharacterCustomizationSystem
{
    private readonly LayeredAnimationController _animationController;
    private readonly IAsepriteAnimationService _asepriteService;
    private readonly ContentManager _contentManager;
    private readonly Dictionary<string, CharacterLayerDefinition> _layerDefinitions = new();
    private readonly Dictionary<string, Texture2D> _loadedTextures = new();

    private CharacterAppearance _currentAppearance = CharacterAppearance.Default;

    public CharacterCustomizationSystem(
        LayeredAnimationController animationController,
        ContentManager contentManager,
        IAsepriteAnimationService asepriteService)
    {
        _animationController = animationController;
        _contentManager = contentManager;
        _asepriteService = asepriteService;
    }

    public void InitializeLayers(Dictionary<string, CharacterLayerDefinition> layerDefinitions)
    {
        _layerDefinitions.Clear();
        foreach (var (name, definition) in layerDefinitions)
        {
            _layerDefinitions[name] = definition;
        }

        // Load initial character with default selections
        LoadCharacterAppearance(CharacterAppearance.Default);
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

    private void LoadLayer(string layerName, int variantIndex)
    {
        CharacterLayerDefinition layerDef = _layerDefinitions[layerName];
        var assetName = layerDef.AvailableAssets[variantIndex];

        // Load texture
        var textureKey = $"{layerName}_{variantIndex}";
        if (!_loadedTextures.ContainsKey(textureKey))
        {
            _loadedTextures[textureKey] = _contentManager.Load<Texture2D>($"Characters/{layerName}/{assetName}");
        }

        // Convert to animations based on format
        Dictionary<string, LDtkAnimationData> animations;

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
        foreach (var (layerName, variantIndex) in appearance.LayerSelections)
        {
            LoadLayer(layerName, variantIndex);
        }
    }

    public CharacterAppearance GetCurrentAppearance()
    {
        return _currentAppearance;
    }
}
