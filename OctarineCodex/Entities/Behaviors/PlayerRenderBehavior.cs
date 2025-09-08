// OctarineCodex/Entities/Behaviors/PlayerRenderBehavior.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Handles player rendering.
/// </summary>
[EntityBehavior(EntityType = "Player", Priority = 100)]
public class PlayerRenderBehavior : EntityBehavior
{
    private readonly Color _playerColor = Color.Red;
    private Texture2D? _playerTexture;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Draw(SpriteBatch? spriteBatch)
    {
        Texture2D? texture = _playerTexture ?? Pixel;
        if (texture == null)
        {
            return;
        }

        spriteBatch?.Draw(
            texture,
            new Rectangle(
                (int)Entity.Position.X,
                (int)Entity.Position.Y,
                PlayerSize,
                PlayerSize),
            _playerColor);
    }
}
