// OctarineCodex/Entities/EntityService.cs (corrected implementation)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

public class EntityService : IEntityService
{
    private readonly List<EntityWrapper> _allEntities = new();
    private readonly EntityBehaviorRegistry _behaviorRegistry;
    private readonly IServiceProvider _services;
    private readonly ILoggingService _logger;

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
        _logger.Debug($"Updating entities for current layer with {levelCount} levels");
        
        _allEntities.Clear();

        foreach (var level in currentLayerLevels)
        {
            var entities = GetAllEntitiesFromLevel(level);
            foreach (var entity in entities)
            {
                var wrapper = new EntityWrapper(entity, _services);
                _behaviorRegistry.ApplyBehaviors(wrapper);
                _allEntities.Add(wrapper);
            }
        }
        
        _logger.Debug($"Updated to {_allEntities.Count} entities for current layer");
    }

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _allEntities)
        {
            entity.Update(gameTime);
        }
    }

    public IEnumerable<EntityWrapper> GetEntitiesOfType(string entityType)
    {
        return _allEntities.Where(e => e.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<T> GetGeneratedEntitiesOfType<T>() where T : ILDtkEntity, new()
    {
        var typeName = typeof(T).Name;
        return _allEntities
            .Where(e => e.EntityType.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            .Select(e => (T)e.UnderlyingEntity);
    }

    public Vector2? GetPlayerSpawnPoint()
    {
        var player = _allEntities.FirstOrDefault(e => e.EntityType == "Player");
        return player?.Position;
    }

    private IEnumerable<ILDtkEntity> GetAllEntitiesFromLevel(LDtkLevel level)
    {
        var entities = new List<ILDtkEntity>();
        
        if (level.LayerInstances == null)
        {
            _logger.Warn($"Level {level.Identifier} has no layer instances");
            return entities;
        }

        // Get the namespace for this level (used by generated entities)
        var levelNamespace = GetLevelNamespace(level);
        var entityTypes = GetEntityTypesInNamespace(levelNamespace);

        foreach (var entityType in entityTypes)
        {
            try
            {
                // Use reflection to call the generic GetEntities<T> method
                var method = typeof(LDtkLevel).GetMethod("GetEntities")?.MakeGenericMethod(entityType);
                if (method != null)
                {
                    var entityArray = method.Invoke(level, null) as Array;
                    if (entityArray != null)
                    {
                        foreach (ILDtkEntity entity in entityArray)
                        {
                            entities.Add(entity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, $"Failed to get entities of type {entityType.Name} from level {level.Identifier}");
            }
        }

        _logger.Debug($"Found {entities.Count} entities in level {level.Identifier}");
        return entities;
    }

    private string GetLevelNamespace(LDtkLevel level)
    {
        // Extract namespace from level identifier
        // This matches the generated code structure: Room2, Production, etc.
        return level.Identifier;
    }

    private IEnumerable<Type> GetEntityTypesInNamespace(string namespaceName)
    {
        try
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == namespaceName && 
                           typeof(ILDtkEntity).IsAssignableFrom(t) &&
                           !t.IsInterface && !t.IsAbstract);
        }
        catch (Exception ex)
        {
            _logger.Exception(ex, $"Failed to get entity types for namespace {namespaceName}");
            return [];
        }
    }
}