// Updated OctarineCodex/Entities/EntityService.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Json;
using OctarineCodex.Logging;
using OctarineCodex.Messaging;

namespace OctarineCodex.Entities;

[UsedImplicitly]
public class EntityService(
    EntityBehaviorRegistry behaviorRegistry,
    ILoggingService logger,
    IMessageBus messageBus,
    IEntityWrapperFactory entityWrapperFactory)
    : IEntityService, IDisposable
{
    private readonly List<EntityWrapper> _allEntities = [];


    /// <summary>
    ///     Cleanup all entities and messaging when shutting down.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        DisposeAllEntities();
        messageBus.Clear();
    }

    public void InitializeEntities(IEnumerable<LDtkLevel> levels)
    {
        var levelCount = levels.Count();
        logger.Debug($"Initializing entities from {levelCount} levels");

        // Clear existing entities and message bus registrations
        DisposeAllEntities();

        foreach (var level in levels)
        {
            var entities = GetAllEntitiesFromLevel(level);
            foreach (var entity in entities)
            {
                EntityWrapper wrapper = entityWrapperFactory.CreateWrapper(entity);
                behaviorRegistry.ApplyBehaviors(wrapper);
                _allEntities.Add(wrapper);

                logger.Debug($"Initialized entity {wrapper.EntityType} at {wrapper.Position}");
            }
        }

        logger.Debug($"Initialized {_allEntities.Count} total entities");
    }

    public void UpdateEntitiesForCurrentLayer(IEnumerable<LDtkLevel> currentLayerLevels)
    {
        var levelCount = currentLayerLevels.Count();
        logger.Debug($"Updating entities for current layer with {levelCount} levels (preserving player)");

        // Preserve the current player entity
        var currentPlayer = GetPlayerEntity();

        // Dispose non-player entities
        List<EntityWrapper> entitiesToDispose = GetAllEntities().Where(e => e != currentPlayer).ToList();
        foreach (EntityWrapper entity in entitiesToDispose)
        {
            entity.Dispose();
        }

        // Clear and reload other entities
        _allEntities.Clear();

        // Re-add the preserved player first
        _allEntities.Add(currentPlayer);
        logger.Debug($"Preserved player entity at {currentPlayer.Position}");

        // Load non-player entities from new layer
        foreach (var level in currentLayerLevels)
        {
            var entities = GetAllEntitiesFromLevel(level);
            foreach (var entity in entities)
            {
                // Skip player entities from LDTK data since we preserved the current one
                if (entity is { Identifier: OctarineConstants.PlayerEntityName })
                {
                    logger.Debug($"Skipping Player entity from LDTK data at {entity.Position}");
                    continue;
                }

                EntityWrapper wrapper = entityWrapperFactory.CreateWrapper(entity);
                behaviorRegistry.ApplyBehaviors(wrapper);
                _allEntities.Add(wrapper);
            }
        }

        logger.Debug($"Updated to {_allEntities.Count} entities for current layer (player preserved)");
    }

    public void Update(GameTime gameTime)
    {
        // Process queued messages first
        messageBus.ProcessQueuedMessages();

        // Create snapshot to prevent concurrent modification during teleport
        var entitySnapshot = _allEntities.ToList();
        foreach (EntityWrapper entity in entitySnapshot)
        {
            entity.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (EntityWrapper entity in _allEntities)
        {
            entity.Draw(spriteBatch);
        }
    }

    public IEnumerable<T> GetGeneratedEntitiesOfType<T>()
        where T : ILDtkEntity, new()
    {
        var typeName = typeof(T).Name;
        var matchingEntities = _allEntities
            .Where(e => e.EntityType.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        logger.Debug(
            $"GetGeneratedEntitiesOfType<{typeName}>: Found {matchingEntities.Count} matching entities out of {_allEntities.Count} total entities");

        var results = new List<T>();
        foreach (var entity in matchingEntities)
        {
            try
            {
                var castedEntity = (T)entity.UnderlyingEntity;
                results.Add(castedEntity);
                logger.Debug(
                    $"Successfully cast entity {entity.EntityType} at {entity.Position} to type {typeof(T).Name}");
            }
            catch (InvalidCastException ex)
            {
                logger.Warn(
                    $"Failed to cast entity {entity.EntityType} (underlying type: {entity.UnderlyingEntity.GetType().FullName}) to {typeof(T).FullName}: {ex.Message}");
            }
        }

        return results;
    }

    public IEnumerable<EntityWrapper> GetAllEntities()
    {
        return _allEntities.AsReadOnly();
    }

    public Vector2? GetPlayerSpawnPoint()
    {
        return GetPlayerEntity().Position;
    }

    public EntityWrapper GetPlayerEntity()
    {
        return GetAllEntities().First(e => e.EntityType == OctarineConstants.PlayerEntityName);
    }


    private void DisposeAllEntities()
    {
        foreach (EntityWrapper entity in _allEntities)
        {
            entity.Dispose();
        }

        _allEntities.Clear();
    }

    private IEnumerable<ILDtkEntity> GetAllEntitiesFromLevel(LDtkLevel level)
    {
        var entities = new List<ILDtkEntity>();

        if (level.LayerInstances == null)
        {
            logger.Warn($"Level {level.Identifier} has no layer instances");
            return entities;
        }

        var targetNamespace = Path.GetFileNameWithoutExtension(level.WorldFilePath);
        var entityTypes = GetValidEntityTypes(targetNamespace);

        foreach (var entityType in entityTypes)
        {
            try
            {
                // Use our Newtonsoft.Json-based loader
                var method = typeof(NewtonsoftEntityLoader).GetMethod("GetEntitiesWithNewtonsoft")
                    ?.MakeGenericMethod(entityType);
                if (method == null)
                {
                    continue;
                }

                if (method.Invoke(null, [level]) is not Array { Length: > 0 } entityArray)
                {
                    continue;
                }

                logger.Debug($"Found {entityArray.Length} entities of type {entityType.Name}");
                entities.AddRange(entityArray.Cast<ILDtkEntity>());
            }
            catch (TargetInvocationException ex)
            {
                logger.Exception(
                    ex,
                    $"Failed to get entities of type {entityType.Name} from level {level.Identifier}");
            }
        }

        logger.Debug($"Found {entities.Count} total entities in level {level.Identifier}");
        return entities;
    }

    private List<Type> GetValidEntityTypes(string targetNamespace)
    {
        try
        {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            var validTypes = allTypes
                .Where(t => typeof(ILDtkEntity).IsAssignableFrom(t) &&
                            t is { IsInterface: false, IsAbstract: false })
                .ToList();

            logger.Debug(
                $"Found {validTypes.Count} valid entity types in namespace {targetNamespace}: {string.Join(", ", validTypes.Select(t => t.Name))}");
            return validTypes;
        }
        catch (Exception ex)
        {
            logger.Exception(ex, "Failed to get entity types from assembly");
            return [];
        }
    }
}
