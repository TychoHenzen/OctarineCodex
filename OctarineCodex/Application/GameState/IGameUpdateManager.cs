using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Application.GameState;

[Service<GameUpdateManager>]
public interface IGameUpdateManager
{
    bool ShouldExit { get; }
    void Update(GameTime gameTime);
}
