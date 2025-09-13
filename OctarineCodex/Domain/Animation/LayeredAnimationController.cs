// Domain/Animation/LayeredAnimationController.cs

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Manages multiple synchronized animation layers for character customization.
///     Each layer (base, clothing, armor, weapon) can have its own animation while staying synchronized.
/// </summary>
public class LayeredAnimationController : IAnimationComponent
{
    private readonly Dictionary<string, Dictionary<string, LDtkAnimationData>> _layerAnimations = [];
    private readonly Dictionary<string, AnimationLayer> _layers = [];

    public bool IsComplete => _layers.Values.All(layer => layer.IsComplete);
    public bool IsPlaying { get; private set; } = true;

    public string CurrentState { get; private set; } = "Idle";

    public void Update(GameTime gameTime)
    {
        foreach (AnimationLayer layer in _layers.Values)
        {
            layer.Update(gameTime);
        }
    }

    public void PlayAnimation(string animationName)
    {
        CurrentState = animationName;
        IsPlaying = true;

        // Set animation for all layers that have this state
        foreach (var (layerName, layer) in _layers)
        {
            if (_layerAnimations[layerName].TryGetValue(animationName, out LDtkAnimationData animation))
            {
                layer.SetAnimation(animation);
            }
        }
    }

    public void StopAnimation()
    {
        IsPlaying = false;
        foreach (AnimationLayer layer in _layers.Values)
        {
            layer.Stop();
        }
    }

    // IAnimationComponent implementation for compatibility
    public int GetCurrentFrame()
    {
        return _layers.Values.FirstOrDefault()?.GetCurrentFrame() ?? 0;
    }

    public int GetCurrentTileId()
    {
        return _layers.Values.FirstOrDefault()?.GetCurrentTileId() ?? 0;
    }

    /// <summary>
    ///     Adds an animation layer (e.g., "Base", "Clothing", "Armor", "Weapon").
    /// </summary>
    public void AddLayer(string layerName, Dictionary<string, LDtkAnimationData> animations, int priority = 0)
    {
        _layerAnimations[layerName] = animations;
        _layers[layerName] = new AnimationLayer(layerName, priority);

        // Start with the current state if this layer has it
        if (animations.ContainsKey(CurrentState))
        {
            _layers[layerName].SetAnimation(animations[CurrentState]);
        }
    }

    /// <summary>
    ///     Removes an animation layer (e.g., when armor is removed).
    /// </summary>
    public void RemoveLayer(string layerName)
    {
        _layers.Remove(layerName);
        _layerAnimations.Remove(layerName);
    }

    /// <summary>
    ///     Gets animation data for a specific layer, sorted by render priority.
    /// </summary>
    public IEnumerable<LayerRenderData> GetLayerRenderData()
    {
        return _layers.Values
            .Where(layer => layer.IsVisible)
            .OrderBy(layer => layer.Priority)
            .Select(layer => new LayerRenderData(
                layer.Name,
                layer.GetCurrentTileId(),
                layer.Priority,
                layer.Alpha));
    }
}

/// <summary>
///     Individual animation layer within a layered animation system.
/// </summary>
public class AnimationLayer
{
    private readonly SimpleAnimationComponent _animation = new();

    public AnimationLayer(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }

    public string Name { get; }
    public int Priority { get; }
    public bool IsVisible { get; set; } = true;
    public float Alpha { get; set; } = 1.0f;
    public bool IsComplete => _animation.IsComplete;

    public void SetAnimation(LDtkAnimationData animationData)
    {
        _animation.SetAnimation(animationData);
    }

    public void Update(GameTime gameTime)
    {
        _animation.Update(gameTime);
    }

    public void Stop()
    {
        _animation.StopAnimation();
    }

    public int GetCurrentFrame()
    {
        return _animation.GetCurrentFrame();
    }

    public int GetCurrentTileId()
    {
        return _animation.GetCurrentTileId();
    }
}

/// <summary>
///     Data for rendering a single animation layer.
/// </summary>
public readonly record struct LayerRenderData(
    string LayerName,
    int TileId,
    int Priority,
    float Alpha);
