using System;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Components;

/// <summary>
///     Rendering component that defines how an entity should be drawn.
///     Contains texture and rendering parameters for MonoGame SpriteBatch.
/// </summary>
public struct RenderComponent
{
    /// <summary>
    ///     Name of the texture asset to render. Must be loaded in ContentManager.
    /// </summary>
    public string TextureAssetName;

    /// <summary>
    ///     Source rectangle within the texture. If null, uses entire texture.
    /// </summary>
    public Rectangle? SourceRectangle;

    /// <summary>
    ///     Tint color applied to the texture. Default is White (no tint).
    /// </summary>
    public Color TintColor;

    /// <summary>
    ///     Render layer depth for sorting. Higher values render behind lower values.
    ///     Range: 0.0f (front) to 1.0f (back).
    /// </summary>
    public float LayerDepth;

    /// <summary>
    ///     Whether this entity should be rendered.
    /// </summary>
    public bool IsVisible;

    /// <summary>
    ///     Creates a new render component with the specified texture.
    /// </summary>
    /// <param name="textureAssetName">Name of the texture asset</param>
    /// <param name="tintColor">Tint color (default: White)</param>
    /// <param name="layerDepth">Layer depth (default: 0.5f)</param>
    /// <param name="sourceRectangle">Source rectangle (default: null)</param>
    /// <param name="isVisible">Whether to render (default: true)</param>
    public RenderComponent(
        string textureAssetName,
        Color tintColor = default,
        float layerDepth = 0.5f,
        Rectangle? sourceRectangle = null,
        bool isVisible = true)
    {
        TextureAssetName = textureAssetName ?? throw new ArgumentNullException(nameof(textureAssetName));
        SourceRectangle = sourceRectangle;
        TintColor = tintColor == default(Color) ? Color.White : tintColor;
        LayerDepth = MathHelper.Clamp(layerDepth, 0f, 1f);
        IsVisible = isVisible;
    }

    /// <summary>
    ///     Creates a render component for sprite sheet animation.
    /// </summary>
    /// <param name="textureAssetName">Sprite sheet texture name</param>
    /// <param name="frameX">Frame X coordinate in the sprite sheet</param>
    /// <param name="frameY">Frame Y coordinate in the sprite sheet</param>
    /// <param name="frameWidth">Width of each frame</param>
    /// <param name="frameHeight">Height of each frame</param>
    /// <param name="tintColor">Tint color</param>
    /// <param name="layerDepth">Layer depth</param>
    public static RenderComponent ForSpriteSheet(
        string textureAssetName,
        int frameX,
        int frameY,
        int frameWidth,
        int frameHeight,
        Color tintColor = default,
        float layerDepth = 0.5f)
    {
        var sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
        return new RenderComponent(textureAssetName, tintColor, layerDepth, sourceRect);
    }
}
