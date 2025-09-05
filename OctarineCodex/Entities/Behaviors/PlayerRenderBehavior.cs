using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Handles player rendering
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 100)]
public class PlayerRenderBehavior : EntityBehavior
{
    private readonly Color _playerColor = Color.Red;
    private Texture2D _playerTexture;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        base.Initialize(entity, services);
        // You could inject a texture service here if needed
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var texture = _playerTexture ?? OctarineConstants.Pixel;
        if (texture == null)
            return;
        var color = _playerColor;

        spriteBatch.Draw(texture,
            new Rectangle((int)Entity.Position.X, (int)Entity.Position.Y, OctarineConstants.PlayerSize,
                OctarineConstants.PlayerSize),
            color);
    }
}