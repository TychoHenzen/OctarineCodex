// OctarineCodex/Application/Entities/EcsEntityFactory.cs

using System.Collections.Generic;
using DefaultEcs;
using LDtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Components;
using OctarineCodex.Infrastructure.Ecs;

namespace OctarineCodex.Application.Entities;

/// <summary>
///     Creates ECS entities from LDtk data while maintaining compatibility with EntityWrapper approach.
///     Provides parallel entity creation during migration phase.
/// </summary>
[Service<EcsEntityFactory>(ServiceLifetime.Scoped)]
public class EcsEntityFactory
{
    private readonly WorldManager _worldManager;

    public EcsEntityFactory(WorldManager worldManager)
    {
        _worldManager = worldManager;
    }

    /// <summary>
    ///     Creates an ECS entity from LDtk entity instance data.
    /// </summary>
    public Entity CreateEntityFromLdtk(EntityInstance ldtkEntity, LDtkLevel level)
    {
        Entity entity = _worldManager.CreateEntity();

        // Convert to world coordinates by adding level offset
        var worldPosition = new Vector2(
            level.WorldX + ldtkEntity.Px.X,
            level.WorldY + ldtkEntity.Px.Y
        );

        // Add core components
        entity.Set(new PositionComponent(worldPosition));

        entity.Set(new LdtkComponent(
            ldtkEntity.Iid.ToString(),
            ldtkEntity._Identifier,
            level.Iid,
            ldtkEntity._Grid.X,
            ldtkEntity._Grid.Y,
            ldtkEntity.Px.X,
            ldtkEntity.Px.Y));

        // Add rendering if entity has a tile
        if (ldtkEntity._Tile != null)
        {
            TilesetRectangle? tile = ldtkEntity._Tile;
            entity.Set(new RenderComponent(
                "tileset", // Configure based on your tileset setup
                sourceRectangle: new Rectangle(tile.X, tile.Y, tile.W, tile.H),
                tintColor: ldtkEntity._SmartColor));
        }

        // Add collision for entities with collision data
        if (NeedsCollision(ldtkEntity._Identifier))
        {
            entity.Set(new CollisionComponent(
                CollisionShapeType.Rectangle,
                new Vector2(ldtkEntity.Width, ldtkEntity.Height)));
        }

        return entity;
    }

    /// <summary>
    ///     Creates ECS entities from all entity instances in a level.
    /// </summary>
    public Entity[] CreateEntitiesFromLevel(LDtkLevel level)
    {
        var entities = new List<Entity>();

        if (level.LayerInstances == null)
        {
            return entities.ToArray();
        }

        foreach (LayerInstance? layer in level.LayerInstances)
        {
            foreach (EntityInstance? entityInstance in layer.EntityInstances)
            {
                Entity entity = CreateEntityFromLdtk(entityInstance, level);
                entities.Add(entity);
            }
        }

        return entities.ToArray();
    }

    /// <summary>
    ///     Creates ECS entities for specific entity types only.
    /// </summary>
    public Entity[] CreateEntitiesOfType(LDtkLevel level, string entityIdentifier)
    {
        var entities = new List<Entity>();

        if (level.LayerInstances == null)
        {
            return entities.ToArray();
        }

        foreach (LayerInstance? layer in level.LayerInstances)
        {
            foreach (EntityInstance? entityInstance in layer.EntityInstances)
            {
                if (entityInstance._Identifier == entityIdentifier)
                {
                    Entity entity = CreateEntityFromLdtk(entityInstance, level);
                    entities.Add(entity);
                }
            }
        }

        return entities.ToArray();
    }

    private static bool NeedsCollision(string entityIdentifier)
    {
        // Configure based on your entity types
        return entityIdentifier is "Player" or "Enemy" or "Obstacle";
    }
}
