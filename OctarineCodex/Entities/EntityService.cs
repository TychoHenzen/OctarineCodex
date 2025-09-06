// OctarineCodex/Entities/EntityService.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Json;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

namespace OctarineCodex.Entities;

public class EntityService : IEntityService
{
    private readonly List<EntityWrapper> _allEntities = new();
    private readonly EntityBehaviorRegistry _behaviorRegistry;
    private readonly ILoggingService _logger;
    private readonly IServiceProvider _services;

    public EntityService(EntityBehaviorRegistry behaviorRegistry, IServiceProvider services, ILoggingService logger)
    {
        _behaviorRegistry = behaviorRegistry ?? throw new ArgumentNullException(nameof(behaviorRegistry));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void InitializeEntities(IEnumerable<LDtkLevel> levels)
    {
        var levelCount = levels.Count();
        _logger.Debug($"Initializing entities from {levelCount} levels");

        _allEntities.Clear();

        foreach (var level in levels)
        {
            var entities = GetAllEntitiesFromLevel(level);
            foreach (var entity in entities)
            {
                var wrapper = new EntityWrapper(entity, _services);
                _behaviorRegistry.ApplyBehaviors(wrapper);
                _allEntities.Add(wrapper);

                _logger.Debug($"Initialized entity {wrapper.EntityType} at {wrapper.Position}");
            }
        }

        _logger.Debug($"Initialized {_allEntities.Count} total entities");
    }

    public void UpdateEntitiesForCurrentLayer(IEnumerable<LDtkLevel> currentLayerLevels)
    {
        var levelCount = currentLayerLevels.Count();
        _logger.Debug($"Updating entities for current layer with {levelCount} levels (preserving player)");

        // Preserve the current player entity
        var currentPlayer = GetPlayerEntity();

        // Clear and reload other entities
        _allEntities.Clear();

        // Re-add the preserved player first
        if (currentPlayer != null)
        {
            _allEntities.Add(currentPlayer);
            _logger.Debug($"Preserved player entity at {currentPlayer.Position}");
        }

        // Load non-player entities from new layer
        foreach (var level in currentLayerLevels)
        {
            var entities = GetAllEntitiesFromLevel(level);
            foreach (var entity in entities)
            {
                // Skip player entities from LDTK data since we preserved the current one
                if (entity is ILDtkEntity ldtkEntity && ldtkEntity.Identifier == "Player")
                {
                    _logger.Debug($"Skipping Player entity from LDTK data at {entity.Position}");
                    continue;
                }

                var wrapper = new EntityWrapper(entity, _services);
                _behaviorRegistry.ApplyBehaviors(wrapper);
                _allEntities.Add(wrapper);
            }
        }

        _logger.Debug($"Updated to {_allEntities.Count} entities for current layer (player preserved)");
    }

    public void Update(GameTime gameTime)
    {
        // Create snapshot to prevent concurrent modification during teleport
        var entitySnapshot = _allEntities.ToList();
        foreach (var entity in entitySnapshot) entity.Update(gameTime);
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var entity in _allEntities) entity.Draw(spriteBatch);
    }

    public IEnumerable<T> GetGeneratedEntitiesOfType<T>() where T : ILDtkEntity, new()
    {
        var typeName = typeof(T).Name;
        var matchingEntities = _allEntities
            .Where(e => e.EntityType.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.Debug(
            $"GetGeneratedEntitiesOfType<{typeName}>: Found {matchingEntities.Count} matching entities out of {_allEntities.Count} total entities");

        var results = new List<T>();
        foreach (var entity in matchingEntities)
            try
            {
                var castedEntity = (T)entity.UnderlyingEntity;
                results.Add(castedEntity);
                _logger.Debug(
                    $"Successfully cast entity {entity.EntityType} at {entity.Position} to type {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                _logger.Warn(
                    $"Failed to cast entity {entity.EntityType} (underlying type: {entity.UnderlyingEntity.GetType().FullName}) to {typeof(T).FullName}: {ex.Message}");
            }

        return results;
    }

    public IEnumerable<EntityWrapper> GetAllEntities()
    {
        return _allEntities.AsReadOnly();
    }

    public Vector2? GetPlayerSpawnPoint()
    {
        var player = _allEntities.FirstOrDefault(e => e.EntityType == "Player");
        return player?.Position;
    }

    public EntityWrapper GetPlayerEntity()
    {
        return _allEntities.FirstOrDefault(e => e.EntityType == "Player");
    }

    public void ApplyBehaviorsToEntity(EntityWrapper entity)
    {
        _behaviorRegistry.ApplyBehaviors(entity);
    }

    private IEnumerable<ILDtkEntity> GetAllEntitiesFromLevel(LDtkLevel level)
    {
        var entities = new List<ILDtkEntity>();

        if (level.LayerInstances == null)
        {
            _logger.Warn($"Level {level.Identifier} has no layer instances");
            return entities;
        }

        var targetNamespace = ExtractNamespaceFromFile(level.WorldFilePath);
        var entityTypes = GetValidEntityTypes(targetNamespace);

        foreach (var entityType in entityTypes)
            try
            {
                // Use our Newtonsoft.Json-based loader
                var method = typeof(NewtonsoftEntityLoader).GetMethod("GetEntitiesWithNewtonsoft")
                    ?.MakeGenericMethod(entityType);
                if (method != null)
                {
                    var entityArray = method.Invoke(null, new object[] { level }) as Array;
                    if (entityArray != null && entityArray.Length > 0)
                    {
                        _logger.Debug($"Found {entityArray.Length} entities of type {entityType.Name}");
                        foreach (ILDtkEntity entity in entityArray) entities.Add(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Exception(ex,
                    $"Failed to get entities of type {entityType.Name} from level {level.Identifier}");
            }

        _logger.Debug($"Found {entities.Count} total entities in level {level.Identifier}");
        return entities;
    }

    private static string ExtractNamespaceFromFile(string ldtkFile)
    {
        // Extract filename without extension to determine namespace
        // e.g., "test_level2.ldtk" -> "test_level2"
        var fileName = Path.GetFileNameWithoutExtension(ldtkFile);
        return fileName;
    }

    private IEnumerable<Type> GetValidEntityTypes(string targetNamespace)
    {
        try
        {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            var validTypes = allTypes
                .Where(t => typeof(ILDtkEntity).IsAssignableFrom(t) &&
                            !t.IsInterface &&
                            !t.IsAbstract &&
                            IsGeneratedEntityType(t, targetNamespace))
                .ToList();

            _logger.Debug(
                $"Found {validTypes.Count} valid entity types in namespace {targetNamespace}: {string.Join(", ", validTypes.Select(t => t.Name))}");
            return validTypes;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex, "Failed to get entity types from assembly");
            return [];
        }
    }

    private static bool IsGeneratedEntityType(Type type, string targetNamespace)
    {
        // Only include types from the specific target namespace
        var isInTargetNamespace = type.Namespace == targetNamespace;
        var isNotEntityWrapper = type.Name != "EntityWrapper";
        var hasParameterlessConstructor = type.GetConstructor(Type.EmptyTypes) != null;

        return isInTargetNamespace && isNotEntityWrapper && hasParameterlessConstructor;
    }
}