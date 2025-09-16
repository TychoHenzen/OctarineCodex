using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Application.GameState;

[Service<GameRenderManager>]
public interface IGameRenderManager
{
    void Draw(SpriteBatch spriteBatch, Vector2 playerPosition);
    Matrix GetWorldTransformMatrix();
}
