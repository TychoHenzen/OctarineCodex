// Application/GameManagement/IGameInitializationManager.cs

using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Application.GameState;

[Service<GameInitializationManager>]
public interface IGameInitializationManager
{
    bool IsWorldLoaded { get; }
    Task<bool> InitializeWorldAsync(GraphicsDevice graphicsDevice, ContentManager content, string worldName);
}
