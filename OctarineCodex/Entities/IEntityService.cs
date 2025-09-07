using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex.Entities;

public interface IEntityService
{
    void InitializeEntities(IEnumerable<LDtkLevel> levels);
    void UpdateEntitiesForCurrentLayer(IEnumerable<LDtkLevel> currentLayerLevels);
    Vector2? GetPlayerSpawnPoint();
    EntityWrapper GetPlayerEntity();
    IEnumerable<T> GetGeneratedEntitiesOfType<T>() where T : ILDtkEntity, new();
    IEnumerable<EntityWrapper> GetAllEntities();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}