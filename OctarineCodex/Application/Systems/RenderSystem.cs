using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Components;
using OctarineCodex.Infrastructure.Ecs;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Infrastructure.MonoGame;

namespace OctarineCodex.Application.Systems;

/// <summary>
///     ECS system that handles rendering of entities with PositionComponent and RenderComponent.
///     Integrates with MonoGame SpriteBatch and provides depth sorting and culling.
/// </summary>
[Service<RenderSystem>(ServiceLifetime.Scoped)]
public class RenderSystem : AEntitySetSystem<float>, ISystem
{
    private readonly IContentManagerService _contentManagerService; // ← CHANGED from ContentManager
    private readonly ILoggingService _logger;
    private readonly Dictionary<string, Texture2D> _textureCache = [];
    private bool _disposed;
    private SpriteBatch? _spriteBatch;

    public RenderSystem(
        WorldManager worldManager,
        IContentManagerService contentManagerService, // ← CHANGED from ContentManager
        ILoggingService logger)
        : base(worldManager.CurrentWorld.GetEntities().With<PositionComponent>().With<RenderComponent>().AsSet())
    {
        _contentManagerService =
            contentManagerService ?? throw new ArgumentNullException(nameof(contentManagerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Update method - RenderSystem doesn't need update logic in Phase 1.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Render system doesn't need update logic in basic implementation
        // Animation systems will be added in later phases
    }

    /// <summary>
    ///     Draws all entities with position and render components.
    /// </summary>
    public void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null)
        {
            _logger.Warn("SpriteBatch not set. Call SetSpriteBatch before Draw.");
            return;
        }

        // Use DefaultEcs system Update method to process entities
        Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    /// <summary>
    ///     Sets the SpriteBatch to use for rendering.
    ///     Must be called before Draw operations.
    /// </summary>
    public void SetSpriteBatch(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
    }

    /// <summary>
    ///     Processes rendering for a single entity.
    ///     Called by DefaultEcs system for each entity matching the query.
    /// </summary>
    protected override void Update(float deltaTime, in Entity entity)
    {
        if (_spriteBatch == null)
        {
            return;
        }

        ref readonly PositionComponent position = ref entity.Get<PositionComponent>();
        ref readonly RenderComponent render = ref entity.Get<RenderComponent>();

        // Skip invisible entities
        if (!render.IsVisible)
        {
            return;
        }

        // Get or load texture
        Texture2D? texture = GetTexture(render.TextureAssetName);
        if (texture == null)
        {
            _logger.Warn($"Failed to load texture: {render.TextureAssetName}");
            return;
        }

        // Calculate destination rectangle
        Rectangle sourceRect = render.SourceRectangle ?? texture.Bounds;
        var origin = new Vector2(sourceRect.Width * 0.5f, sourceRect.Height * 0.5f);

        try
        {
            _spriteBatch.Draw(
                texture,
                position.Position,
                render.SourceRectangle,
                render.TintColor,
                position.Rotation,
                origin,
                position.Scale,
                SpriteEffects.None,
                render.LayerDepth
            );
        }
        catch (Exception ex)
        {
            _logger.Exception(ex, $"Error rendering entity with texture {render.TextureAssetName}");
        }
    }

    /// <summary>
    ///     Gets a texture from cache or loads it from content manager.
    /// </summary>
    private Texture2D? GetTexture(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        if (_textureCache.TryGetValue(assetName, out Texture2D? cachedTexture))
        {
            return cachedTexture;
        }

        try
        {
            var texture = _contentManagerService.Load<Texture2D>(assetName); // ← CHANGED to use service
            _textureCache[assetName] = texture;
            return texture;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex, $"Failed to load texture: {assetName}");
            return null;
        }
    }


    /// <summary>
    ///     Gets diagnostic information about the render system.
    /// </summary>
    public RenderSystemDiagnostics GetDiagnostics()
    {
        return new RenderSystemDiagnostics
        {
            CachedTextureCount = _textureCache.Count, CachedTextureNames = _textureCache.Keys.ToArray()
        };
    }

    /// <summary>
    ///     Dispose method that properly cleans up resources.
    ///     Since AEntitySetSystem already implements IDisposable, we override its Dispose method.
    /// </summary>
    public new void Dispose()
    {
        if (!_disposed)
        {
            // Don't dispose cached textures - they're managed by ContentManager
            _textureCache.Clear();
            _logger.Debug("RenderSystem disposed");
            _disposed = true;
        }

        // Call base dispose
        base.Dispose();
    }
}

/// <summary>
///     Diagnostic information about the render system.
/// </summary>
public class RenderSystemDiagnostics
{
    public int CachedTextureCount { get; init; }
    public string[] CachedTextureNames { get; init; } = [];
}
