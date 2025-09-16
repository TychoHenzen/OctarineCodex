// OctarineCodex\Application\GameState\GameRenderManager.cs

using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Maps;
using OctarineCodex.Presentation.Camera;
using OctarineCodex.Presentation.Rendering;

namespace OctarineCodex.Application.GameState;

public class GameRenderManager(
    ILevelRenderer levelRenderer,
    ICameraService cameraService,
    IWorldLayerService worldLayerService,
    IEntityService entityService,
    IMapService mapService) : IGameRenderManager
{
    public Matrix GetWorldTransformMatrix()
    {
        return cameraService.GetTransformMatrix();
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
    {
        // Only render if world is loaded
        if (!mapService.IsLoaded)
        {
            return;
        }

        // Get player entity for positioning
        EntityWrapper? player = entityService.GetPlayerEntity();
        Vector2 actualPlayerPosition = player?.Position ?? playerPosition;

        // Get current layer levels for rendering
        IReadOnlyList<LDtkLevel> currentLayerLevels = worldLayerService.GetCurrentLayerLevels();

        // Render background and collision layers (behind player)
        levelRenderer.RenderLevelsBeforePlayer(
            currentLayerLevels,
            spriteBatch,
            actualPlayerPosition);

        // Render entities at correct depth (includes teleport indicators via behaviors)
        entityService.Draw(spriteBatch);

        // Render wall tiles in front of player (Y-sorted)
        levelRenderer.RenderLevelsAfterPlayer(
            currentLayerLevels,
            spriteBatch,
            actualPlayerPosition);

        // Render foreground tiles (always on top)
        levelRenderer.RenderForegroundLayers(
            currentLayerLevels,
            spriteBatch,
            actualPlayerPosition);
    }
}
