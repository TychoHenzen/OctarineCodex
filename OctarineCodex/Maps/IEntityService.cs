using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

public interface IEntityService
{
    void InitializeEntities(IEnumerable<LDtkLevel> levels);
    void UpdateEntitiesForCurrentLayer(IEnumerable<LDtkLevel> currentLayerLevels);
    Vector2? GetPlayerSpawnPoint();
    IEnumerable<EntityData> GetEntitiesOfType(string entityType);
    IEnumerable<T> GetGeneratedEntitiesOfType<T>() where T : ILDtkEntity, new();
}