using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

public interface IEntityService
{
    void InitializeEntities(IEnumerable<LDtkLevel> levels);
    Vector2? GetPlayerSpawnPoint();
    IEnumerable<EntityData> GetEntitiesOfType(string entityType);
}