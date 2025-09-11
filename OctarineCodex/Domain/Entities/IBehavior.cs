// OctarineCodex/Entities/Behaviors/IBehavior.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Entities.Behaviors;

public interface IBehavior
{
    void Initialize(EntityWrapper entity);
    void Update(GameTime gameTime);

    void OnMessage<T>(T message)
        where T : class;
    void Cleanup();

    void Draw(SpriteBatch spriteBatch);
}
