using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Entities;
using OctarineCodex.Maps;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
/// Renders a visual indicator when the player is near a teleport.
/// </summary>
[EntityBehavior(EntityType = "Teleport", Priority = 200)]
[UsedImplicitly]
public class TeleportIndicatorBehavior(
    IEntityService entityService,
    ITeleportService teleportService,
    IMapService mapService)
    : EntityBehavior
{
    private const float InteractionDistance = 48f;
    private float _pulseTime;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        // Apply to any entity with "Teleport" in its type name (covers both "Teleport" and "Teleporter")
        return entity.EntityType.ToLowerInvariant().Contains("teleport");
    }

    public override void Update(GameTime gameTime)
    {
        // Update pulse animation timer
        _pulseTime = (float)gameTime.TotalGameTime.TotalSeconds;
    }

    public override void Draw(SpriteBatch? spriteBatch)
    {
        if (spriteBatch == null || Pixel == null)
        {
            return;
        }

        // Only render indicators in multi-level worlds
        if (mapService.CurrentLevels.Count <= 1)
        {
            return;
        }

        // Check if player is nearby
        EntityWrapper? player = entityService.GetPlayerEntity();
        if (player == null)
        {
            return;
        }

        var distance = Vector2.Distance(player.Position, Entity.Position);
        if (distance > InteractionDistance)
        {
            return;
        }

        // Check if this teleport is actually available
        if (!teleportService.IsTeleportAvailable(player.Position, out _, out _))
        {
            return;
        }

        // Draw pulsing indicator
        DrawIndicator(spriteBatch);
    }

    private void DrawIndicator(SpriteBatch spriteBatch)
    {
        // Calculate pulsing intensity
        var pulseIntensity = 0.5f + (0.5f * (float)Math.Sin(_pulseTime * 4.0));
        Color indicatorColor = Color.Cyan * pulseIntensity;

        // Draw indicator above the teleport
        var indicatorRect = new Rectangle(
            (int)(Entity.Position.X - 12),
            (int)(Entity.Position.Y - 16),
            (int)Entity.Size.X + 24,
            4);

        spriteBatch.Draw(Pixel, indicatorRect, indicatorColor);

        // Draw a subtle glow around the teleport entity itself
        var glowAlpha = 0.3f * pulseIntensity;
        Color glowColor = Color.Cyan * glowAlpha;

        // Draw glow rectangles around the entity
        var glowThickness = 2;

        // Top
        spriteBatch.Draw(Pixel, new Rectangle(
            (int)Entity.Position.X - glowThickness,
            (int)Entity.Position.Y - glowThickness,
            (int)Entity.Size.X + (glowThickness * 2),
            glowThickness), glowColor);

        // Bottom
        spriteBatch.Draw(Pixel, new Rectangle(
            (int)Entity.Position.X - glowThickness,
            (int)(Entity.Position.Y + Entity.Size.Y),
            (int)Entity.Size.X + (glowThickness * 2),
            glowThickness), glowColor);

        // Left
        spriteBatch.Draw(Pixel, new Rectangle(
            (int)Entity.Position.X - glowThickness,
            (int)Entity.Position.Y,
            glowThickness,
            (int)Entity.Size.Y), glowColor);

        // Right
        spriteBatch.Draw(Pixel, new Rectangle(
            (int)(Entity.Position.X + Entity.Size.X),
            (int)Entity.Position.Y,
            glowThickness,
            (int)Entity.Size.Y), glowColor);
    }
}
