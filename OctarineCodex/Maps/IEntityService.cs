using System;
using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

public interface IEntityService
{
    void InitializeEntities(IEnumerable<LDtkLevel> levels);
    Vector2? GetPlayerSpawnPoint();
    IEnumerable<EntityData> GetEntitiesOfType(string entityType);
}

public class EntityData
{
    public string Type { get; set; } = "";
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class EntityService : IEntityService
{
    private readonly List<EntityData> _entities = new();

    public void InitializeEntities(IEnumerable<LDtkLevel> levels)
    {
        _entities.Clear();

        foreach (var level in levels)
        {
            if (level.LayerInstances == null) continue;

            var entityLayers = level.LayerInstances.Where(l => l._Type == LayerType.Entities);

            foreach (var layer in entityLayers)
            foreach (var entity in layer.EntityInstances)
            {
                var entityData = new EntityData
                {
                    Type = entity._Identifier,
                    Position = new Vector2(level.WorldX + entity.Px.X, level.WorldY + entity.Px.Y),
                    Size = new Vector2(entity.Width, entity.Height)
                };

                // Extract properties from field instances
                foreach (var field in entity.FieldInstances) entityData.Properties[field._Identifier] = field._Value;

                _entities.Add(entityData);
            }
        }
    }

    public Vector2? GetPlayerSpawnPoint()
    {
        var playerSpawn = _entities.FirstOrDefault(e =>
            e.Type.Equals("Player", StringComparison.OrdinalIgnoreCase) ||
            e.Type.Equals("PlayerSpawn", StringComparison.OrdinalIgnoreCase));

        return playerSpawn?.Position;
    }

    public IEnumerable<EntityData> GetEntitiesOfType(string entityType)
    {
        return _entities.Where(e => e.Type.Equals(entityType, StringComparison.OrdinalIgnoreCase));
    }
}