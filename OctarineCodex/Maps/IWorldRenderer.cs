using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Maps;

public interface IWorldRenderer : ISimpleLevelRenderer
{
    void RenderWorld(IEnumerable<LDtkLevel> levels, SpriteBatch spriteBatch, Camera2D camera);
}