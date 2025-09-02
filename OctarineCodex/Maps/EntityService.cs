using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LDtk;
using Microsoft.Xna.Framework;
using Room4;

namespace OctarineCodex.Maps;

public class EntityService : IEntityService
{
    private readonly List<EntityData> _entities = new();
    private readonly List<ILDtkEntity> _generatedEntities = new();
    private IReadOnlyList<LDtkLevel> _allLevels = new List<LDtkLevel>();

    public void InitializeEntities(IEnumerable<LDtkLevel> levels)
    {
        _allLevels = levels.ToList();
        // Don't load entities yet - wait for UpdateEntitiesForCurrentLayer
        _entities.Clear();
        _generatedEntities.Clear();
    }

    public void UpdateEntitiesForCurrentLayer(IEnumerable<LDtkLevel> currentLayerLevels)
    {
        _entities.Clear();
        _generatedEntities.Clear();

        foreach (var level in currentLayerLevels)
        {
            if (level.LayerInstances == null) continue;

            var entityLayers = level.LayerInstances.Where(l => l._Type == LayerType.Entities);

            foreach (var layer in entityLayers)
            foreach (var entityInstance in layer.EntityInstances)
            {
                // Create legacy EntityData for backward compatibility
                var entityData = new EntityData
                {
                    Type = entityInstance._Identifier,
                    Position = new Vector2(level.WorldX + entityInstance.Px.X, level.WorldY + entityInstance.Px.Y),
                    Size = new Vector2(entityInstance.Width, entityInstance.Height)
                };

                // Extract properties from field instances
                foreach (var field in entityInstance.FieldInstances) 
                    entityData.Properties[field._Identifier] = field._Value;

                _entities.Add(entityData);

                // Create strongly-typed generated entity
                var generatedEntity = CreateGeneratedEntity(entityInstance, level);
                if (generatedEntity != null)
                {
                    _generatedEntities.Add(generatedEntity);
                }
            }
        }
    }

    private ILDtkEntity? CreateGeneratedEntity(EntityInstance entityInstance, LDtkLevel level)
    {
        // Map entity types to their generated classes
        return entityInstance._Identifier switch
        {
            "Teleport" => CreateTeleport(entityInstance, level),
            "Player" => CreatePlayer(entityInstance, level),
            "Item" => CreateItem(entityInstance, level),
            // Add other entity types as needed
            _ => null
        };
    }

    private Teleport CreateTeleport(EntityInstance entityInstance, LDtkLevel level)
    {
        var teleport = new Teleport
        {
            Identifier = entityInstance._Identifier,
            Iid = entityInstance.Iid,
            Uid = entityInstance.DefUid,
            Position = new Vector2(level.WorldX + entityInstance.Px.X, level.WorldY + entityInstance.Px.Y),
            Size = new Vector2(entityInstance.Width, entityInstance.Height),
            Pivot = new Vector2(entityInstance._Pivot.X, entityInstance._Pivot.Y),
            SmartColor = entityInstance._SmartColor
        };

        // Extract the destination field using the correct property name and JsonElement deserialization
        var destinationField = entityInstance.FieldInstances
            .FirstOrDefault(f => f._Identifier == "destination");
        
        if (destinationField != null && destinationField._Value.ValueKind == JsonValueKind.Object)
        {
            try
            {
                // Deserialize JsonElement to EntityReference
                var entityRef = JsonSerializer.Deserialize<EntityReference>(destinationField._Value);
                if (entityRef != null)
                {
                    teleport.destination = entityRef;
                }
            }
            catch (JsonException ex)
            {
                // Handle deserialization error gracefully
                Console.WriteLine($"Failed to deserialize EntityReference for teleport: {ex.Message}");
            }
        }

        return teleport;
    }

    private Room4.Player CreatePlayer(EntityInstance entityInstance, LDtkLevel level)
    {
        var player = new Room4.Player
        {
            Identifier = entityInstance._Identifier,
            Iid = entityInstance.Iid,
            Uid = entityInstance.DefUid,
            Position = new Vector2(level.WorldX + entityInstance.Px.X, level.WorldY + entityInstance.Px.Y),
            Size = new Vector2(entityInstance.Width, entityInstance.Height),
            Pivot = new Vector2(entityInstance._Pivot.X, entityInstance._Pivot.Y),
            SmartColor = entityInstance._SmartColor
        };

        // Extract HP field using JsonElement
        var hpField = entityInstance.FieldInstances
            .FirstOrDefault(f => f._Identifier == "HP");
        if (hpField != null && hpField._Value.ValueKind == JsonValueKind.Number)
        {
            if (hpField._Value.TryGetInt32(out var hp))
            {
                player.HP = hp;
            }
        }

        return player;
    }

    private Item CreateItem(EntityInstance entityInstance, LDtkLevel level)
    {
        // Similar pattern for Item entities
        return new Item
        {
            Identifier = entityInstance._Identifier,
            Iid = entityInstance.Iid,
            Uid = entityInstance.DefUid,
            Position = new Vector2(level.WorldX + entityInstance.Px.X, level.WorldY + entityInstance.Px.Y),
            Size = new Vector2(entityInstance.Width, entityInstance.Height),
            Pivot = new Vector2(entityInstance._Pivot.X, entityInstance._Pivot.Y),
            SmartColor = entityInstance._SmartColor
        };
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

    public IEnumerable<T> GetGeneratedEntitiesOfType<T>() where T : ILDtkEntity, new()
    {
        return _generatedEntities.OfType<T>();
    }
}